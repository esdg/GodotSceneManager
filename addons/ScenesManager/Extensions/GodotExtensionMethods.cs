using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
using System.IO;

namespace MoF.Addons.ScenesManager.Extensions
{
    /// <summary>
    /// Provides extension methods for working with Godot nodes and resources.
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
            Type[] parmTypes = parms?.Select(p => p.ParameterType).ToArray() ?? Array.Empty<Type>();

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

            // Get all signals from signal list
            var signalList = source.GetSignalList();
            GD.Print($"[SceneManagerEditor] Total signals in signal list: {signalList.Count}");

            // Get all C# events
            EventInfo[] events = source.GetType().GetEvents();
            GD.Print($"[SceneManagerEditor] Total C# events: {events.Length}");

            for (int i = 0; i < signalList.Count; i++)
            {
                var signal = signalList[i];
                var firstValue = signal.Values.FirstOrDefault();
                var signalName = firstValue.VariantType == Variant.Type.String ? firstValue.AsString() : "Unknown";
                bool isCSharpSignal = IsCSharpSignal(source, signalName);
                string signalType = isCSharpSignal ? "C#" : "GDScript";

                GD.Print($"[SceneManagerEditor] Signal {i}: '{signalName}' - Type: {signalType}");

                if (isCSharpSignal)
                {
                    var eventInfo = events.FirstOrDefault(e => e.Name == signalName);
                    if (eventInfo != null)
                    {
                        GD.Print($"[SceneManagerEditor]   -> EventHandlerType: {eventInfo.EventHandlerType?.Name}");
                    }
                }
            }
        }
#nullable disable
    }
}
