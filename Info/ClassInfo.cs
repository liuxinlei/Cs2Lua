﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Semantics;

namespace RoslynTool.CsToLua
{
    internal class ClassInfo
    {
        internal bool IsEnum = false;
        internal bool IsEntryClass = false;
        internal bool IsValueType = false;
        internal bool IsInnerOfGenericType = false;

        internal string Key = string.Empty;
        internal string BaseKey = string.Empty;
        internal string GenericTypeKey = string.Empty;

        internal string ExportConstructor = string.Empty;
        internal MethodInfo ExportConstructorInfo = null;
        internal HashSet<string> References = new HashSet<string>();
        internal HashSet<string> IgnoreReferences = new HashSet<string>();
                
        internal bool ExistConstructor = false;
        internal bool ExistStaticConstructor = false;

        internal INamedTypeSymbol SemanticInfo = null;
        internal ClassSymbolInfo ClassSemanticInfo = null;

        internal StringBuilder CurrentCodeBuilder = null;

        internal StringBuilder BeforeOuterCodeBuilder = new StringBuilder();
        internal StringBuilder AfterOuterCodeBuilder = new StringBuilder();

        internal StringBuilder InstanceFunctionCodeBuilder = new StringBuilder();
        internal StringBuilder InstanceFieldCodeBuilder = new StringBuilder();
        internal StringBuilder InstancePropertyCodeBuilder = new StringBuilder();
        internal StringBuilder InstanceEventCodeBuilder = new StringBuilder();

        internal StringBuilder StaticFunctionCodeBuilder = new StringBuilder();
        internal StringBuilder StaticFieldCodeBuilder = new StringBuilder();
        internal StringBuilder StaticPropertyCodeBuilder = new StringBuilder();
        internal StringBuilder StaticEventCodeBuilder = new StringBuilder();

        internal StringBuilder InstanceInitializerCodeBuilder = new StringBuilder();
        internal StringBuilder StaticInitializerCodeBuilder = new StringBuilder();
        internal StringBuilder EnumValue2StringCodeBuilder = new StringBuilder();

        internal Dictionary<string, StringBuilder> ExtensionCodeBuilders = new Dictionary<string, StringBuilder>();

        internal Dictionary<string, ClassInfo> InnerClasses = new Dictionary<string, ClassInfo>();

        internal void Init(INamedTypeSymbol sym, ClassSymbolInfo info)
        {
            IsEnum = sym.TypeKind == TypeKind.Enum;
            IsEntryClass = HasAttribute(sym, "Cs2Lua.EntryAttribute");
            IsValueType = sym.IsValueType;

            IsInnerOfGenericType = IsInnerClassOfGenericType(sym);

            ExistConstructor = false;
            ExistStaticConstructor = false;
            SemanticInfo = sym;
            ClassSemanticInfo = info;
                        
            Key = GetFullName(sym);
            BaseKey = GetFullName(sym.BaseType);
            if (BaseKey == "System.Object" || BaseKey == "System.ValueType") {
                BaseKey = string.Empty;
            }

            GenericTypeKey = ClassInfo.GetFullNameWithTypeParameters(sym);

            References.Clear();
            IgnoreReferences.Clear();
        }
        internal void AddReference(ISymbol sym)
        {
            var arrType = sym as IArrayTypeSymbol;
            if (null != arrType) {
                AddReference(arrType.ElementType);
            } else {
                var refType = sym as INamedTypeSymbol;
                if (null == refType) {
                    refType = sym.ContainingType;
                }
                if (null != refType) {
                    AddReference(refType);
                } else {
                    Logger.Instance.ReportIllegalType(sym);
                }
            }
        }
        internal void AddReference(INamedTypeSymbol refType)
        {
            if (!IsInnerClassOfGenericType(refType)) {
                while (null != refType.ContainingType) {
                    refType = refType.ContainingType;
                }
            }
            string key = GetFullName(refType);
            if (null != refType && refType != SemanticInfo && refType.ContainingAssembly == SemanticInfo.ContainingAssembly && !refType.IsAnonymousType && !refType.IsImplicitClass && !refType.IsImplicitlyDeclared && refType.TypeKind != TypeKind.Delegate && refType.TypeKind != TypeKind.Dynamic && refType.TypeKind != TypeKind.Interface) {
                if (!string.IsNullOrEmpty(key) && !References.Contains(key) && key != Key) {
                    bool isIgnore = ClassInfo.HasAttribute(refType, "Cs2Lua.IgnoreAttribute");
                    if (isIgnore) {
                        IgnoreReferences.Add(key);
                    } else {
                        if (!SemanticInfo.IsGenericType || SemanticInfo.TypeArguments.IndexOf(refType) < 0) {
                            References.Add(key);
                            if (refType.IsGenericType) {
                                foreach (var sym in refType.TypeArguments) {
                                    AddReference(sym);
                                }
                            }
                        }
                    }
                }
            }
        }
        internal bool IsInherit(INamedTypeSymbol type)
        {
            bool ret = false;
            if (null != SemanticInfo) {
                var baseType = SemanticInfo.BaseType;
                while (null != baseType) {
                    if (type == baseType) {
                        ret = true;
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
            return ret;
        }
        internal static bool IsInnerClassOfGenericType(INamedTypeSymbol type)
        {
            bool ret = false;
            var refType = type;
            while (null != refType.ContainingType) {
                refType = refType.ContainingType;
                if (refType.IsGenericType) {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        internal static bool IsOriginalOrContainingGenericType(INamedTypeSymbol type, INamedTypeSymbol genericType)
        {
            bool ret = false;
            if (type.OriginalDefinition == genericType) {
                ret = true;
            } else {
                var refType = type.OriginalDefinition;
                while (null != refType.ContainingType) {
                    refType = refType.ContainingType;
                    if (refType==genericType) {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        internal static bool IsBaseInitializerCalled(ConstructorDeclarationSyntax node, SemanticModel model)
        {
            bool baseInitializerCalled = false;
            var init = node.Initializer;
            if (null != init) {
                var oper = model.GetOperation(init) as IInvocationExpression;
                if (init.ThisOrBaseKeyword.Text == "this") {
                    var constructor = oper.TargetMethod;
                    if (null != constructor) {
                        var decl = constructor.DeclaringSyntaxReferences;
                        if (decl.Length == 1) {
                            var syntax = decl[0].GetSyntax() as ConstructorDeclarationSyntax;
                            if (null != syntax) {
                                return IsBaseInitializerCalled(syntax, model);
                            }
                        }
                    }
                } else if (init.ThisOrBaseKeyword.Text == "base") {
                    baseInitializerCalled = true;
                }
            }
            return baseInitializerCalled;
        }
        
        internal static bool HasAttribute(ISymbol sym, string fullName)
        {
            if (null == sym)
                return false;
            foreach (var attr in sym.GetAttributes()) {
                string fn = GetFullName(attr.AttributeClass);
                if (fn == fullName)
                    return true;
            }
            return false;
        }
        internal static T GetAttributeArgument<T>(ISymbol sym, string fullName, string argName)
        {
            if (null == sym)
                return default(T);
            foreach (var attr in sym.GetAttributes()) {
                string fn = GetFullName(attr.AttributeClass);
                if (fn == fullName) {
                    var args = attr.NamedArguments;
                    foreach (var pair in args) {
                        if (pair.Key == argName) {
                            var arg = pair.Value;
                            return (T)Convert.ChangeType(arg.Value, typeof(T));
                        }
                    }
                }
            }
            return default(T);
        }
        internal static T GetAttributeArgument<T>(ISymbol sym, string fullName, int index)
        {
            if (null == sym)
                return default(T);
            foreach (var attr in sym.GetAttributes()) {
                string fn = GetFullName(attr.AttributeClass);
                if (fn == fullName) {
                    var args = attr.ConstructorArguments;
                    int ct = args.Length;
                    if (index >= 0 && index < ct) {
                        var arg = args[index];
                        return (T)Convert.ChangeType(arg.Value, typeof(T));
                    }
                }
            }
            return default(T);
        }

        internal static string GetFullName(ISymbol type)
        {
            if (null == type)
                return string.Empty;
            if (type.ContainingAssembly == SymbolTable.Instance.AssemblySymbol) {
                return CalcFullName(type, true);
            } else {
                //外部类型不会基于泛型样式导入，只有使用lua实现的集合类会出现这种情况，这里需要用泛型类型名以与utility.lua里的名称一致
                return CalcFullNameWithTypeParameters(type, true);
            }
        }
        internal static string GetNamespaces(ISymbol type)
        {
            if (null == type)
                return string.Empty;
            if (type.Kind == SymbolKind.Namespace) {
                return CalcFullName(type, true);
            } else {
                return CalcFullName(type, false);
            }
        }        
        internal static string GetFullNameWithTypeParameters(ISymbol type)
        {
            if (null == type)
                return string.Empty;
            return CalcFullNameWithTypeParameters(type, true);
        }
        internal static string GetNamespacesWithTypeParameters(ISymbol type)
        {
            if (null == type)
                return string.Empty;
            if (type.Kind == SymbolKind.Namespace) {
                return CalcFullNameWithTypeParameters(type, true);
            } else {
                return CalcFullNameWithTypeParameters(type, false);
            }
        }

        internal static string CalcNameWithFullTypeName(string name, INamedTypeSymbol typeSym)
        {
            if (null == typeSym) {
                return name;
            } else {
                string ns = CalcNameWithTypeParameters(typeSym);
                if (string.IsNullOrEmpty(ns)) {
                    return name;
                } else {
                    return ns.Replace(".", "_") + "_" + name;
                }
            }
        }

        private static string CalcFullName(ISymbol type, bool includeSelfName)
        {
            if (null == type)
                return string.Empty;
            List<string> list = new List<string>();
            if (includeSelfName) {
                list.Add(CalcNameWithTypeArguments(type));
            }
            INamespaceSymbol ns = type.ContainingNamespace;
            var ct = type.ContainingType;
            string name = string.Empty;
            if (null != ct) {
                name = CalcNameWithTypeArguments(ct);
            }
            while (null != ct && name.Length > 0) {
                list.Insert(0, name);
                ns = ct.ContainingNamespace;
                ct = ct.ContainingType;
                if (null != ct) {
                    name = CalcNameWithTypeArguments(ct);
                } else {
                    name = string.Empty;
                }
            }
            while (null != ns && ns.Name.Length > 0) {
                list.Insert(0, ns.Name);
                ns = ns.ContainingNamespace;
            }
            return string.Join(".", list.ToArray());
        }
        private static string CalcFullNameWithTypeParameters(ISymbol type, bool includeSelfName)
        {
            if (null == type)
                return string.Empty;
            List<string> list = new List<string>();
            if (includeSelfName) {
                list.Add(CalcNameWithTypeParameters(type));
            }
            INamespaceSymbol ns = type.ContainingNamespace;
            var ct = type.ContainingType;
            string name = string.Empty;
            if (null != ct) {
                name = CalcNameWithTypeParameters(ct);
            }
            while (null != ct && name.Length > 0) {
                list.Insert(0, name);
                ns = ct.ContainingNamespace;
                ct = ct.ContainingType;
                if (null != ct) {
                    name = CalcNameWithTypeParameters(ct);
                } else {
                    name = string.Empty;
                }
            }
            while (null != ns && ns.Name.Length > 0) {
                list.Insert(0, ns.Name);
                ns = ns.ContainingNamespace;
            }
            return string.Join(".", list.ToArray());
        }

        private static string CalcNameWithTypeParameters(ISymbol sym)
        {
            if (null == sym)
                return string.Empty;
            var typeSym = sym as INamedTypeSymbol;
            if (null != typeSym) {
                return CalcNameWithTypeParameters(typeSym);
            } else {
                return sym.Name;
            }
        }
        private static string CalcNameWithTypeArguments(ISymbol sym)
        {
            if (null == sym)
                return string.Empty;
            var typeSym = sym as INamedTypeSymbol;
            if (null != typeSym) {
                return CalcNameWithTypeArguments(typeSym);
            } else {
                return sym.Name;
            }
        }
        private static string CalcNameWithTypeParameters(INamedTypeSymbol type)
        {
            if (null == type)
                return string.Empty;
            List<string> list = new List<string>();
            list.Add(type.Name);
            foreach (var param in type.TypeParameters) {
                list.Add(param.Name);
            }
            return string.Join("_", list.ToArray());
        }
        private static string CalcNameWithTypeArguments(INamedTypeSymbol type)
        {
            if (null == type)
                return string.Empty;
            List<string> list = new List<string>();
            list.Add(type.Name);
            foreach (var arg in type.TypeArguments) {
                if (arg.TypeKind == TypeKind.TypeParameter) {
                    var t = SymbolTable.Instance.FindTypeArgument(arg);
                    if (t.TypeKind == TypeKind.TypeParameter) {
                        list.Add(t.Name);
                    } else {
                        var fn = GetFullName(t);
                        list.Add(fn.Replace(".", "_"));
                    }
                } else {
                    var fn = GetFullName(arg);
                    list.Add(fn.Replace(".", "_"));
                }
            }
            return string.Join("_", list.ToArray());
        }        
    }
}
