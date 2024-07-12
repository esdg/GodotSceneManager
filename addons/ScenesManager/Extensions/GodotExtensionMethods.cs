using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
#nullable enable
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

		public static void ConnectToStaticDelegate<T>(this GodotObject source, object targetInstance, string signalName, string staticTargetMethodName, params object?[]? args)
		{
			EventInfo? eventInfo = source?.GetType().GetEvent(signalName);
			if (eventInfo == null)
			{
				GD.PrintErr($"Event '{signalName}' not found on source object.");
				return;
			}

			Type? handlerType = eventInfo.EventHandlerType;
			MethodInfo? invokeMethod = handlerType?.GetMethod("Invoke");
			ParameterInfo[]? parms = invokeMethod?.GetParameters();
			Type[] parmTypes = parms?.Select(p => p.ParameterType).ToArray() ?? Array.Empty<Type>();

			AssemblyName aName = new AssemblyName("DynamicTypes");
			AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
			ModuleBuilder mb = ab.DefineDynamicModule(aName.Name);
			TypeBuilder tb = mb.DefineType("Handler", TypeAttributes.Class | TypeAttributes.Public);

			FieldBuilder sourceNodeField = tb.DefineField("sourceNode", typeof(Node), FieldAttributes.Public);
			FieldBuilder sceneManagerOutSlotSignalField = tb.DefineField("sceneManagerOutSlotSignal", typeof(T), FieldAttributes.Public);

			ConstructorBuilder constructor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(Node), typeof(T) });
			ILGenerator ctorIL = constructor.GetILGenerator();
			ctorIL.Emit(OpCodes.Ldarg_0);
			ctorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
			ctorIL.Emit(OpCodes.Ldarg_0);
			ctorIL.Emit(OpCodes.Ldarg_1);
			ctorIL.Emit(OpCodes.Stfld, sourceNodeField);
			ctorIL.Emit(OpCodes.Ldarg_0);
			ctorIL.Emit(OpCodes.Ldarg_2);
			ctorIL.Emit(OpCodes.Stfld, sceneManagerOutSlotSignalField);
			ctorIL.Emit(OpCodes.Ret);

			MethodBuilder handler = tb.DefineMethod("DynamicHandler",
				MethodAttributes.Public, invokeMethod?.ReturnType, parmTypes);

			ILGenerator ilgen = handler.GetILGenerator();
			MethodInfo? targetMethod = targetInstance.GetType().GetMethod(staticTargetMethodName, new Type[] { typeof(Node), typeof(T) });

			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Ldfld, sourceNodeField);
			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Ldfld, sceneManagerOutSlotSignalField);
			ilgen.EmitCall(OpCodes.Call, targetMethod, null);
			ilgen.Emit(OpCodes.Ret);

			Type? finished = tb.CreateType();
			MethodInfo? eventHandler = finished?.GetMethod("DynamicHandler");

			var inst = Activator.CreateInstance(finished, args);

			if (handlerType is not null && eventHandler is not null)
			{
				Delegate d = Delegate.CreateDelegate(handlerType, inst, eventHandler);
				eventInfo?.AddEventHandler(source, d);
			}
		}
	}
}
