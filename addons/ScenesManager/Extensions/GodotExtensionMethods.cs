using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
using Godot.Collections;
using System.IO;

namespace MoF.Addons.ScenesManager.Extensions
{
    /// <summary>
    /// Provides extension methods for working with Godot nodes and resources.
    /// 
    /// Dynamic Built-in Signal Detection:
    /// This class automatically detects built-in signals by analyzing the inheritance chain
    /// of Godot objects. It identifies signals that come from base Godot classes (like Node, Control, etc.)
    /// and considers them built-in, while user-defined signals are those added in your custom classes.
    /// 
    /// Usage examples:
    /// - someNode.GetUserDefinedSignals() - Gets only user-defined signals
    /// - someNode.DebugSignals() - Shows detailed signal analysis
    /// - someNode.GetDetectedBuiltInSignals() - Shows what signals are detected as built-in
    /// - GodotExtensionMethodsNode.ClearBuiltInSignalsCache() - Clears the cache if needed
    /// </summary>
    public static class GodotExtensionMethodsNode
    {
        /// <summary>
        /// Removes the specified children from the node and frees them.
        /// </summary>
        /// <param name="node">The node from which to remove the children.</param>
        /// <param name="targetNodes">The array of nodes to remove.</param>
        public static void RemoveChildren(this Node node, Node[] targetNodes)
        {
            foreach (Node n in targetNodes)
            {
                node.RemoveChild(n);
                n.QueueFree();
            }
        }

        /// <summary>
        /// Gets the directory path of the specified resource using System.IO for robustness.
        /// </summary>
        /// <param name="resource">The resource from which to get the path.</param>
        /// <returns>The directory path of the resource.</returns>
        public static string GetPath(this Resource resource)
        {
            if (string.IsNullOrEmpty(resource.ResourcePath))
                return string.Empty;
            return Path.GetDirectoryName(resource.ResourcePath.Replace("/", Path.DirectorySeparatorChar.ToString()))?.Replace(Path.DirectorySeparatorChar, '/')
                   ?? string.Empty;
        }

        /// <summary>
        /// Removes all children from the node and frees them.
        /// </summary>
        /// <param name="node">The node to clear.</param>
        public static void RemoveAllChildren(this Node node)
        {
            foreach (Node child in node.GetChildren())
            {
                node.RemoveChild(child);
                child.QueueFree();
            }
        }

#nullable enable
        /// <summary>
        /// Gets the index of a signal by name from a GodotObject.
        /// </summary>
        /// <param name="source">The GodotObject to search for the signal.</param>
        /// <param name="name">The name of the signal to find.</param>
        /// <returns>The index of the signal if found, otherwise -1.</returns>
        public static int GetSignalIndex(this GodotObject source, string name)
        {
            if (source == null)
            {
                GD.PrintErr("[SceneManagerEditor] Source object cannot be null");
                return -1;
            }

            if (string.IsNullOrEmpty(name))
            {
                GD.PrintErr("[SceneManagerEditor] Signal name cannot be null or empty");
                return -1;
            }

            var signalList = source.GetSignalList();

            for (int i = 0; i < signalList.Count; i++)
            {
                var signal = signalList[i];
                var firstValue = signal.Values.FirstOrDefault();
                if (firstValue.VariantType == Variant.Type.String && firstValue.AsString() == name)
                {
                    return i;
                }
            }

            return -1; // Signal not found
        }

        /// <summary>
        /// Determines if a signal is a C# signal (exists in assembly) or a GDScript signal.
        /// </summary>
        /// <param name="source">The source GodotObject.</param>
        /// <param name="signalName">The name of the signal.</param>
        /// <returns>True if it's a C# signal, false if it's a GDScript signal.</returns>
        private static bool IsCSharpSignal(GodotObject source, string signalName)
        {
            EventInfo[] events = source.GetType().GetEvents();
            return events.Any(e => e.Name == signalName);
        }

        /// <summary>
        /// Gets the EventInfo for a C# signal from a GodotObject.
        /// </summary>
        /// <param name="source">The source GodotObject.</param>
        /// <param name="signalName">The name of the signal.</param>
        /// <returns>The EventInfo if found, otherwise null.</returns>
        private static EventInfo? GetEventInfoFromCSharpSignal(GodotObject source, string signalName)
        {
            if (!IsCSharpSignal(source, signalName))
            {
                GD.PrintErr($"[SceneManagerEditor] Signal '{signalName}' is not a C# signal");
                return null;
            }

            EventInfo[] events = source.GetType().GetEvents();
            return events.FirstOrDefault(e => e.Name == signalName);
        }

        /// <summary>
        /// Cache for built-in signal names per type to avoid repeated reflection calls.
        /// </summary>
        private static readonly System.Collections.Generic.Dictionary<Type, System.Collections.Generic.HashSet<string>> _builtInSignalsCache = new();

        /// <summary>
        /// Determines if a signal is a built-in Godot signal (not user-defined) by checking if it exists
        /// in the inheritance chain of base Godot classes.
        /// </summary>
        /// <param name="source">The source GodotObject to check against.</param>
        /// <param name="signalName">The name of the signal to check.</param>
        /// <returns>True if it's a built-in signal, false if it's user-defined.</returns>
        private static bool IsBuiltInSignal(GodotObject source, string signalName)
        {
            if (source == null || string.IsNullOrEmpty(signalName))
                return false;

            Type sourceType = source.GetType();

            // Check cache first
            if (_builtInSignalsCache.TryGetValue(sourceType, out var cachedSignals))
            {
                return cachedSignals.Contains(signalName);
            }

            // Build cache for this type
            var builtInSignals = GetBuiltInSignalsForType(sourceType);
            _builtInSignalsCache[sourceType] = builtInSignals;

            return builtInSignals.Contains(signalName);
        }

        /// <summary>
        /// Gets all built-in signals for a given type by using multiple detection methods:
        /// 1. Check inheritance chain for C# events
        /// 2. Check against a comprehensive list of known Godot signals
        /// 3. Compare with signals from a fresh instance of base Godot types
        /// </summary>
        /// <param name="type">The type to analyze.</param>
        /// <returns>HashSet of built-in signal names for this type.</returns>
        private static System.Collections.Generic.HashSet<string> GetBuiltInSignalsForType(Type type)
        {
            var builtInSignals = new System.Collections.Generic.HashSet<string>();

            // Method 1: Get events from inheritance chain (existing approach)
            Type? currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                if (IsGodotBaseType(currentType))
                {
                    EventInfo[] events = currentType.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                    foreach (EventInfo eventInfo in events)
                    {
                        builtInSignals.Add(eventInfo.Name);
                    }
                }
                currentType = currentType.BaseType;
            }

            // Method 2: Add comprehensive list of known Godot built-in signals
            // This covers signals that might not be exposed as C# events
            string[] knownBuiltInSignals = {
                // GodotObject level
                "script_changed", "property_list_changed",
                
                // Node level  
                "ready", "tree_entered", "tree_exited", "tree_exiting", "renamed",
                "child_entered_tree", "child_exiting_tree", "replacing_by",
                
                // CanvasItem level
                "draw", "visibility_changed", "hidden", "item_rect_changed",
                
                // Control level
                "resized", "size_flags_changed", "minimum_size_changed",
                "focus_entered", "focus_exited", "gui_input", "mouse_entered", "mouse_exited",
                "theme_changed",
                
                // BaseButton level
                "pressed", "button_down", "button_up", "toggled",
                
                // OptionButton level
                "item_selected", "item_focused",
                
                // LineEdit level
                "text_changed", "text_submitted", "text_change_rejected",
                
                // Range level (Slider, SpinBox, etc.)
                "value_changed",
                
                // ItemList level
                "item_selected", "nothing_selected", "item_activated",
                
                // TextEdit level
                "text_changed", "cursor_changed", "text_set",
                
                // ScrollContainer level
                "scroll_started", "scroll_ended",
                
                // Popup level
                "popup_hide", "about_to_popup",
                
                // Window level
                "window_input", "files_dropped", "mouse_entered", "mouse_exited",
                "focus_entered", "focus_exited", "close_requested", "go_back_requested",
                "visibility_changed", "about_to_popup",
                
                // Animation
                "animation_finished", "animation_changed", "animation_started",
                "tween_started", "tween_step", "tween_completed", "tween_all_completed",
                
                // Physics
                "body_entered", "body_exited", "area_entered", "area_exited",
                "input_event", "mouse_entered", "mouse_exited",
                
                // Audio
                "finished",
                
                // Editor specific (might appear in some contexts)
                "editor_description_changed", "editor_state_changed", "child_order_changed"
            };

            foreach (string signal in knownBuiltInSignals)
            {
                builtInSignals.Add(signal);
            }

            // Method 3: For specific types, try to create a temporary instance and check its signals
            // This is more expensive but catches signals we might have missed
            try
            {
                if (IsDirectGodotType(type))
                {
                    var tempInstance = CreateTemporaryInstance(type);
                    if (tempInstance != null)
                    {
                        var tempSignals = tempInstance.GetSignalList();
                        foreach (Dictionary signal in tempSignals)
                        {
                            var firstValue = signal.Values.FirstOrDefault();
                            if (firstValue.VariantType == Variant.Type.String)
                            {
                                builtInSignals.Add(firstValue.AsString());
                            }
                        }

                        // Clean up temporary instance
                        if (tempInstance is Node node)
                        {
                            node.QueueFree();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If we can't create a temporary instance, that's fine - we'll rely on the other methods
                GD.PrintErr($"[SceneManagerEditor] Could not create temporary instance for type {type.Name}: {ex.Message}");
            }

            return builtInSignals;
        }

        /// <summary>
        /// Checks if a type is a direct Godot type (not a user-derived class).
        /// </summary>
        private static bool IsDirectGodotType(Type type)
        {
            // Check if this is directly a Godot type, not a user-derived type
            string? assemblyName = type.Assembly.GetName().Name;
            return assemblyName?.StartsWith("Godot") == true;
        }

        /// <summary>
        /// Attempts to create a temporary instance of a Godot type for signal detection.
        /// </summary>
        private static GodotObject? CreateTemporaryInstance(Type type)
        {
            try
            {
                // Only try to create instances of common, safe Godot types
                if (type == typeof(Node) || type == typeof(Control) || type == typeof(CanvasItem))
                {
                    return (GodotObject?)Activator.CreateInstance(type);
                }

                // For other types, check if they have a parameterless constructor
                if (type.GetConstructor(Type.EmptyTypes) != null)
                {
                    return (GodotObject?)Activator.CreateInstance(type);
                }
            }
            catch
            {
                // If creation fails, return null
            }

            return null;
        }

        /// <summary>
        /// Determines if a type is a base Godot type (not user-defined).
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if it's a base Godot type, false if it's user-defined.</returns>
        private static bool IsGodotBaseType(Type type)
        {
            // Check if the type is from the Godot assembly
            string? assemblyName = type.Assembly.GetName().Name;
            if (assemblyName?.StartsWith("Godot") == true)
            {
                return true;
            }

            // Check if it's in the Godot namespace
            if (type.Namespace?.StartsWith("Godot") == true)
            {
                return true;
            }

            // Additional check for common Godot base types
            string[] godotBaseTypes = {
                "Node", "Node2D", "Node3D", "Control", "CanvasItem", "CanvasLayer",
                "Resource", "RefCounted", "GodotObject", "PackedScene", "Texture2D",
                "AudioStream", "Shader", "Material", "Mesh", "AnimationPlayer",
                "Timer", "Area2D", "Area3D", "RigidBody2D", "RigidBody3D",
                "StaticBody2D", "StaticBody3D", "CharacterBody2D", "CharacterBody3D"
            };

            return godotBaseTypes.Contains(type.Name);
        }

        /// <summary>
        /// Gets only user-defined signals from a GodotObject, filtering out built-in Godot signals.
        /// </summary>
        /// <param name="source">The source GodotObject.</param>
        /// <param name="includeCSSignals">Whether to include C# signals in the result.</param>
        /// <param name="includeGDScriptSignals">Whether to include GDScript signals in the result.</param>
        /// <returns>Array of signal dictionaries containing only user-defined signals.</returns>
        public static Array<Dictionary> GetUserDefinedSignals(this GodotObject source, bool includeCSSignals = true, bool includeGDScriptSignals = true)
        {
            if (source == null)
            {
                GD.PrintErr("[SceneManagerEditor] Source object cannot be null");
                return new Array<Dictionary>();
            }

            var allSignals = source.GetSignalList();
            var userSignals = new Array<Dictionary>();

            foreach (Dictionary signal in allSignals)
            {
                var firstValue = signal.Values.FirstOrDefault();
                if (firstValue.VariantType == Variant.Type.String)
                {
                    string signalName = firstValue.AsString();

                    // Skip built-in signals
                    if (IsBuiltInSignal(source, signalName))
                        continue;

                    bool isCSharpSignal = IsCSharpSignal(source, signalName);

                    // Apply filters based on signal type
                    if ((isCSharpSignal && includeCSSignals) || (!isCSharpSignal && includeGDScriptSignals))
                    {
                        userSignals.Add(signal);
                    }
                }
            }

            return userSignals;
        }

        /// <summary>
        /// Gets signals from a GodotObject with custom filtering options.
        /// </summary>
        /// <param name="source">The source GodotObject.</param>
        /// <param name="excludeBuiltIn">Whether to exclude built-in Godot signals.</param>
        /// <param name="customExcludeList">Additional signal names to exclude (case-insensitive).</param>
        /// <param name="includeCSSignals">Whether to include C# signals in the result.</param>
        /// <param name="includeGDScriptSignals">Whether to include GDScript signals in the result.</param>
        /// <returns>Array of signal dictionaries based on the filtering criteria.</returns>
        public static Array<Dictionary> GetFilteredSignals(this GodotObject source, bool excludeBuiltIn = true,
            string[]? customExcludeList = null, bool includeCSSignals = true, bool includeGDScriptSignals = true)
        {
            if (source == null)
            {
                GD.PrintErr("[SceneManagerEditor] Source object cannot be null");
                return new Array<Dictionary>();
            }

            var allSignals = source.GetSignalList();
            var filteredSignals = new Array<Dictionary>();
            customExcludeList ??= System.Array.Empty<string>();

            foreach (Dictionary signal in allSignals)
            {
                var firstValue = signal.Values.FirstOrDefault();
                if (firstValue.VariantType == Variant.Type.String)
                {
                    string signalName = firstValue.AsString();

                    // Skip built-in signals if requested
                    if (excludeBuiltIn && IsBuiltInSignal(source, signalName))
                        continue;

                    // Skip custom excluded signals
                    if (customExcludeList.Any(excluded => string.Equals(excluded, signalName, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    bool isCSharpSignal = IsCSharpSignal(source, signalName);

                    // Apply filters based on signal type
                    if ((isCSharpSignal && includeCSSignals) || (!isCSharpSignal && includeGDScriptSignals))
                    {
                        filteredSignals.Add(signal);
                    }
                }
            }

            return filteredSignals;
        }


        /// <summary>
        /// Connects a signal to a static delegate method on a target instance.
        /// Automatically detects whether the signal is a C# signal or GDScript signal and handles accordingly.
        /// </summary>
        /// <typeparam name="T">The type of the static target method parameter.</typeparam>
        /// <param name="source">The source object emitting the signal.</param>
        /// <param name="targetInstance">The instance of the target class containing the static method.</param>
        /// <param name="signalName">The name of the signal to connect.</param>
        /// <param name="staticTargetMethodName">The name of the static method to connect to.</param>
        /// <param name="args">Optional arguments to pass to the static method.</param>
        /// <exception cref="Exception">Thrown if the event, handler type, invoke method, or target method is not found.</exception>
        /// <remarks>
        /// This method uses reflection and dynamic type generation to connect Godot signals to static methods.
        /// Use with caution as it may impact performance and debugging.
        /// </remarks>
        public static void ConnectToStaticDelegate<T>(this GodotObject source, object targetInstance, string signalName, string staticTargetMethodName, params object?[]? args)
        {
            if (source == null)
            {
                GD.PrintErr("[SceneManagerEditor] Source object cannot be null");
                return;
            }

            // Check if signal exists in signal list first
            int signalIndex = source.GetSignalIndex(signalName);
            if (signalIndex < 0)
            {
                GD.PrintErr($"[SceneManagerEditor] Signal '{signalName}' not found on source object");
                return;
            }

            // Determine if this is a C# signal or GDScript signal
            if (IsCSharpSignal(source, signalName))
            {
                ConnectCSharpSignalToStaticDelegate<T>(source, targetInstance, signalName, staticTargetMethodName, args);
            }
            else
            {
                ConnectGDScriptSignalToStaticDelegate<T>(source, targetInstance, signalName, staticTargetMethodName, args);
            }
        }

        /// <summary>
        /// Connects a C# signal to a static delegate method using reflection.
        /// </summary>
        private static void ConnectCSharpSignalToStaticDelegate<T>(GodotObject source, object targetInstance, string signalName, string staticTargetMethodName, params object?[]? args)
        {
            EventInfo? eventInfo = GetEventInfoFromCSharpSignal(source, signalName);
            if (eventInfo == null)
            {
                return; // Error already logged in GetEventInfoFromCSharpSignal
            }

            GD.Print($"[SceneManagerEditor] Connecting C# signal '{signalName}'");

            Type? handlerType = eventInfo.EventHandlerType;
            MethodInfo? invokeMethod = handlerType?.GetMethod("Invoke");
            ParameterInfo[]? parms = invokeMethod?.GetParameters();
            Type[] parmTypes = parms?.Select(p => p.ParameterType).ToArray() ?? System.Array.Empty<Type>();

            AssemblyName assemblyName = new("DynamicTypes");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            if (assemblyName.Name == null)
            {
                throw new Exception($"Assembly name '{assemblyName}' not found on target instance");
            }
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Handler", TypeAttributes.Class | TypeAttributes.Public);

            FieldBuilder sourceNodeField = typeBuilder.DefineField("sourceNode", typeof(Node), FieldAttributes.Public);
            FieldBuilder sceneManagerOutSlotSignalField = typeBuilder.DefineField("sceneManagerOutSlotSignal", typeof(T), FieldAttributes.Public);

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(Node), typeof(T) });
            ILGenerator constructorIL = constructorBuilder.GetILGenerator();

            ConstructorInfo objectConstructor = typeof(object).GetConstructor(Type.EmptyTypes) ?? throw new Exception("Unable to find object constructor");
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Call, objectConstructor);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, sourceNodeField);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_2);
            constructorIL.Emit(OpCodes.Stfld, sceneManagerOutSlotSignalField);
            constructorIL.Emit(OpCodes.Ret);

            MethodBuilder handlerMethodBuilder = typeBuilder.DefineMethod("DynamicHandler", MethodAttributes.Public, invokeMethod?.ReturnType, parmTypes);
            ILGenerator methodIL = handlerMethodBuilder.GetILGenerator();

            MethodInfo? targetMethod = targetInstance.GetType().GetMethod(staticTargetMethodName, new Type[] { typeof(Node), typeof(T) });
            if (targetMethod == null)
            {
                throw new Exception($"Target method '{staticTargetMethodName}' not found on target instance");
            }

            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, sourceNodeField);
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, sceneManagerOutSlotSignalField);
            methodIL.EmitCall(OpCodes.Call, targetMethod, null);
            methodIL.Emit(OpCodes.Ret);

            Type handlerTypeFinished = typeBuilder.CreateType() ?? throw new Exception("Unable to create handler type");
            MethodInfo handlerMethodInfo = handlerTypeFinished.GetMethod("DynamicHandler") ?? throw new Exception("Unable to get DynamicHandler method");
            object? handlerInstance = Activator.CreateInstance(handlerTypeFinished, args);

            if (handlerType is not null)
            {
                Delegate handlerDelegate = Delegate.CreateDelegate(handlerType, handlerInstance, handlerMethodInfo);
                eventInfo.AddEventHandler(source, handlerDelegate);
            }
        }

        /// <summary>
        /// Connects a GDScript signal to a static delegate method using Godot's Connect method.
        /// </summary>
        private static void ConnectGDScriptSignalToStaticDelegate<T>(GodotObject source, object targetInstance, string signalName, string staticTargetMethodName, params object?[]? args)
        {
            GD.Print($"[SceneManagerEditor] Connecting GDScript signal '{signalName}'");

            MethodInfo? targetMethod = targetInstance.GetType().GetMethod(staticTargetMethodName, new Type[] { typeof(Node), typeof(T) });
            if (targetMethod == null)
            {
                throw new Exception($"Target method '{staticTargetMethodName}' not found on target instance");
            }

            // For GDScript signals, we use Godot's Connect method directly
            // Create a callable that wraps our target method
            Callable callable = Callable.From(() =>
            {
                try
                {
                    if (args != null && args.Length >= 2 && args[0] is Node node && args[1] is T param)
                    {
                        targetMethod.Invoke(targetInstance, new object[] { node, param });
                    }
                    else
                    {
                        GD.PrintErr($"[SceneManagerEditor] Invalid arguments for GDScript signal '{signalName}'");
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[SceneManagerEditor] Error invoking target method for GDScript signal '{signalName}': {ex.Message}");
                }
            });

            // Connect the signal using Godot's native connection system
            Error result = source.Connect(signalName, callable);
            if (result != Error.Ok)
            {
                GD.PrintErr($"[SceneManagerEditor] Failed to connect GDScript signal '{signalName}': {result}");
            }
            else
            {
                GD.Print($"[SceneManagerEditor] Successfully connected GDScript signal '{signalName}'");
            }
        }

        /// <summary>
        /// Utility method to debug and list all signals on an object, showing which are C# and which are GDScript.
        /// </summary>
        /// <param name="source">The source GodotObject to analyze.</param>
        public static void DebugSignals(this GodotObject source)
        {
            if (source == null)
            {
                GD.PrintErr("[SceneManagerEditor] Source object cannot be null");
                return;
            }

            GD.Print($"[SceneManagerEditor] Debugging signals for {source.GetType().Name}:");

            // Show inheritance chain
            Type? currentType = source.GetType();
            GD.Print("[SceneManagerEditor] Inheritance chain:");
            int depth = 0;
            while (currentType != null && currentType != typeof(object))
            {
                string indent = new string(' ', depth * 2);
                bool isGodotBase = IsGodotBaseType(currentType);
                string typeInfo = isGodotBase ? " (Godot base type)" : " (User type)";
                GD.Print($"[SceneManagerEditor] {indent}{currentType.Name}{typeInfo}");
                currentType = currentType.BaseType;
                depth++;
            }

            // Get all signals from signal list
            var signalList = source.GetSignalList();
            GD.Print($"[SceneManagerEditor] Total signals in signal list: {signalList.Count}");

            // Get all C# events
            EventInfo[] events = source.GetType().GetEvents();
            GD.Print($"[SceneManagerEditor] Total C# events: {events.Length}");

            // Show detected built-in signals
            var detectedBuiltIns = source.GetDetectedBuiltInSignals();
            GD.Print($"[SceneManagerEditor] Detected built-in signals ({detectedBuiltIns.Length}): {string.Join(", ", detectedBuiltIns)}");

            for (int i = 0; i < signalList.Count; i++)
            {
                var signal = signalList[i];
                var firstValue = signal.Values.FirstOrDefault();
                var signalName = firstValue.VariantType == Variant.Type.String ? firstValue.AsString() : "Unknown";
                bool isCSharpSignal = IsCSharpSignal(source, signalName);
                bool isBuiltIn = IsBuiltInSignal(source, signalName);
                string signalType = isCSharpSignal ? "C#" : "GDScript";
                string signalCategory = isBuiltIn ? "Built-in" : "User-defined";

                GD.Print($"[SceneManagerEditor] Signal {i}: '{signalName}' - Type: {signalType}, Category: {signalCategory}");

                if (isCSharpSignal)
                {
                    var eventInfo = events.FirstOrDefault(e => e.Name == signalName);
                    if (eventInfo != null)
                    {
                        GD.Print($"[SceneManagerEditor]   -> EventHandlerType: {eventInfo.EventHandlerType?.Name}");
                    }
                }
            }

            // Summary statistics
            var userDefinedSignals = source.GetUserDefinedSignals();
            var builtInCount = signalList.Count - userDefinedSignals.Count;
            GD.Print($"[SceneManagerEditor] Summary: {userDefinedSignals.Count} user-defined, {builtInCount} built-in signals");
        }

        /// <summary>
        /// Clears the built-in signals cache. Useful for debugging or if you need to refresh the cache.
        /// </summary>
        public static void ClearBuiltInSignalsCache()
        {
            _builtInSignalsCache.Clear();
            GD.Print("[SceneManagerEditor] Built-in signals cache cleared");
        }

        /// <summary>
        /// Gets all built-in signals detected for a specific object type.
        /// Useful for debugging what signals are being considered built-in.
        /// </summary>
        /// <param name="source">The source GodotObject to analyze.</param>
        /// <returns>Array of built-in signal names for this object type.</returns>
        public static string[] GetDetectedBuiltInSignals(this GodotObject source)
        {
            if (source == null)
            {
                GD.PrintErr("[SceneManagerEditor] Source object cannot be null");
                return System.Array.Empty<string>();
            }

            Type sourceType = source.GetType();

            // Ensure we have the cache for this type
            if (!_builtInSignalsCache.TryGetValue(sourceType, out var cachedSignals))
            {
                cachedSignals = GetBuiltInSignalsForType(sourceType);
                _builtInSignalsCache[sourceType] = cachedSignals;
            }

            return cachedSignals.ToArray();
        }
#nullable disable
    }
}
