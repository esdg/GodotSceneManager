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
        public static int GetSignalIndex(this GodotObject source, string name)
        {
            var signal = source.GetSignalList().FirstOrDefault(s => (string)s.Values.First() == name);
            return source.GetSignalList().IndexOf(signal);
        }


        /// <summary>
        /// Connects a signal to a static delegate method on a target instance.
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

            //EventInfo? eventInfo = source?.GetType().GetEvent(signalName);
            int? signalIndex = source?.GetSignalIndex(signalName);
            EventInfo? eventInfo = null;

            if (signalIndex.HasValue)
            {
                eventInfo = source?.GetType().GetEvents()[signalIndex.Value];
            }
            else
            {
                GD.PrintErr($"Signal '{signalName}' not found on source object");
            }


            if (eventInfo == null)
            {
                GD.PrintErr($"[SceneManagerEditor] Event '{signalName}' not found on source object");
                return;
            }

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
#nullable disable
    }
}
