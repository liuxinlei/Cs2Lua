﻿using System;
using LuaInterface;
using SLua;
using System.Collections.Generic;
public class Lua_UnityEngine_UI_Toggle_ToggleEvent : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent o;
			o=new UnityEngine.UI.Toggle.ToggleEvent();
			pushValue(l,true);
			pushValue(l,o);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int AddListener(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			UnityEngine.Events.UnityAction<System.Boolean> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			self.AddListener(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int RemoveListener(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			UnityEngine.Events.UnityAction<System.Boolean> a1;
			LuaDelegation.checkDelegate(l,2,out a1);
			self.RemoveListener(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int Invoke(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			System.Boolean a1;
			checkType(l,2,out a1);
			self.Invoke(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetPersistentEventCount(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			var ret=self.GetPersistentEventCount();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetPersistentTarget(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			var ret=self.GetPersistentTarget(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int GetPersistentMethodName(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			var ret=self.GetPersistentMethodName(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int SetPersistentListenerState(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			System.Int32 a1;
			checkType(l,2,out a1);
			UnityEngine.Events.UnityEventCallState a2;
			checkEnum(l,3,out a2);
			self.SetPersistentListenerState(a1,a2);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static public int RemoveAllListeners(IntPtr l) {
		try {
			UnityEngine.UI.Toggle.ToggleEvent self=(UnityEngine.UI.Toggle.ToggleEvent)checkSelf(l);
			self.RemoveAllListeners();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	static public void reg(IntPtr l) {
		LuaUnityEvent_bool.reg(l);
		getTypeTable(l,"UnityEngine.UI.Toggle.ToggleEvent");
		addMember(l,AddListener);
		addMember(l,RemoveListener);
		addMember(l,Invoke);
		addMember(l,GetPersistentEventCount);
		addMember(l,GetPersistentTarget);
		addMember(l,GetPersistentMethodName);
		addMember(l,SetPersistentListenerState);
		addMember(l,RemoveAllListeners);
		createTypeMetatable(l,constructor, typeof(UnityEngine.UI.Toggle.ToggleEvent),typeof(LuaUnityEvent_bool));
	}
}
