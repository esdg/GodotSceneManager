using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Godot;

namespace MoF.Addons.ScenesManager.Extensions
{
	public static class GodotExtensionMethodsNode
	{
		public static void RemoveChildren(this Node node, Node[] targetNodes)
		{
			foreach (Node n in targetNodes)
			{
				node.RemoveChild(n);
				n.QueueFree();
			}
		}
		public static string GetPath(this Resource resource)
		{
			return resource.ResourcePath.Substr(0, resource.ResourcePath.Length - resource.ResourcePath.Split("/").Last().Length);
		}

#nullable enable
		public static void ConnectToStaticDelegate<T>(this GodotObject source, object targetInstance, string signalName, string staticTargetMethodName, params object?[]? args)
		{
			EventInfo? eventInfo = source?.GetType().GetEvent(signalName);
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
	}
#nullable disable
}
