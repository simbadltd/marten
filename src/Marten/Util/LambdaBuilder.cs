﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Baseline;

namespace Marten.Util
{
    public static class LambdaBuilder
    {
        public static Func<TTarget, TProperty> GetProperty<TTarget, TProperty>(PropertyInfo property)
        {
            var target = Expression.Parameter(property.ReflectedType, "target");
            var method = property.GetGetMethod();

            var callGetMethod = Expression.Call(target, method);

            var lambda = Expression.Lambda<Func<TTarget, TProperty>>(callGetMethod, target);

            return lambda.Compile();
        }

        public static Action<TTarget, TProperty> SetProperty<TTarget, TProperty>(PropertyInfo property)
        {
            var target = Expression.Parameter(property.ReflectedType, "target");
            var value = Expression.Parameter(property.PropertyType, "value");

            var method = property.GetSetMethod();

            var callSetMethod = Expression.Call(target, method, value);

            var lambda = Expression.Lambda<Action<TTarget, TProperty>>(callSetMethod, target, value);

            return lambda.Compile();
        }


        public static Func<TTarget, TField> GetField<TTarget, TField>(FieldInfo field)
        {
            var target = Expression.Parameter(typeof(TTarget), "target");

            var fieldAccess = Expression.Field(target, field);

            var lambda = Expression.Lambda<Func<TTarget, TField>>(fieldAccess, target);

            return lambda.Compile();
        }

        public static Func<TTarget, TMember> Getter<TTarget, TMember>(MemberInfo member)
        {
            return member is PropertyInfo
                ? GetProperty<TTarget, TMember>(member.As<PropertyInfo>())
                : GetField<TTarget, TMember>(member.As<FieldInfo>());
        }


        public static Action<TTarget, TField> SetField<TTarget, TField>(FieldInfo field)
        {
            var target = Expression.Parameter(typeof(TTarget), "target");
            var value = Expression.Parameter(typeof(TField), "value");

            var fieldAccess = Expression.Field(target, field);
            var fieldSetter = Expression.Assign(fieldAccess, value);

            var lambda = Expression.Lambda<Action<TTarget, TField>>(fieldSetter, target, value);

            return lambda.Compile();
        }


        public static Action<TTarget, TMember> Setter<TTarget, TMember>(MemberInfo member)
        {
            return member is PropertyInfo
                ? SetProperty<TTarget, TMember>(member.As<PropertyInfo>())
                : SetField<TTarget, TMember>(member.As<FieldInfo>());
        }

        public static Func<TTarget, TValue> Getter<TTarget, TValue>(MemberInfo[] members)
        {
            if (members.Length == 1)
            {
                return Getter<TTarget, TValue>(members.Single());
            }

            var target = Expression.Parameter(typeof(TTarget), "target");

            var body = ToExpression(members, target);

            var lambda = Expression.Lambda<Func<TTarget, TValue>>(body, target);

            return lambda.Compile();
        }

        public static Expression ToExpression(MemberInfo[] members, ParameterExpression target)
        {
            Expression body = target;
            foreach (var member in members)
            {
                if (member is PropertyInfo)
                {
                    var propertyInfo = member.As<PropertyInfo>();
                    var getMethod = propertyInfo.GetGetMethod();

                    body = Expression.Call(body, getMethod);
                }
                else
                {
                    var field = member.As<FieldInfo>();
                    body = Expression.Field(body, field);
                }
            }
            return body;
        }
    }
}