using ParcelPrepGov.Web.Features.Bulletin;
using System;
using System.Linq.Expressions;

namespace ParcelPrepGov.Web.Tests
{

    public partial class RuleEngineTests
    {
        public static class RuleManager
        {
            /// <summary>
            /// test code cannot see into this
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="r"></param>
            /// <returns></returns>
            public static Func<T, bool> CompileRule<T>(Rule r)
            {
                var paramContainer = Expression.Parameter(typeof(T));
                Expression expr = BuildExpr<T>(r, paramContainer);
                // build a lambda function T->bool and compile it
                return Expression.Lambda<Func<T, bool>>(expr, paramContainer).Compile();
            }

            /// <summary>
            /// test code cannot see into this
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="r"></param>
            /// <param name="param"></param>
            /// <returns></returns>
            public static Expression BuildExpr<T>(Rule r, ParameterExpression param)
            {
                var left = MemberExpression.Property(param, r.MemberName);
                var tProp = typeof(T).GetProperty(r.MemberName).PropertyType;
                ExpressionType tBinary;
                // is the operator a known .NET operator?
                if (ExpressionType.TryParse(r.Operator, out tBinary))
                {
                    var right = Expression.Constant(Convert.ChangeType(r.TargetValue, tProp));
                    // use a binary operation, e.g. 'Equal' -> 'r.Folder == "CMOP"'
                    return Expression.MakeBinary(tBinary, left, right);
                }
                else
                {
                    var method = tProp.GetMethod(r.Operator);
                    var tParam = method.GetParameters()[0].ParameterType;
                    var right = Expression.Constant(Convert.ChangeType(r.TargetValue, tParam));
                    // use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag)'
                    return Expression.Call(left, method, right);
                }
            }
        }
    }
}
