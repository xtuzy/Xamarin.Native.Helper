//
// Copyright (c) 2017 Lee P. Richardson
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//https://github.com/lprichar/EasyLayout.Droid
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Yang.Maui.Helper.Layouts
{
    public static class EasyLayout
    {
        private enum Position
        {
            Top,
            Right,
            Left,
            Bottom,
            CenterX,
            CenterY,
            Center,
            Constant,
            Width,
            Height,
            Baseline
        }

        private struct LeftExpression
        {
            public View View { get; set; }
            public Position Position { get; set; }
            public string Name { get; set; }
        }

        private struct RightExpression
        {
            public bool IsParent { get; set; }
            public int? Id { get; set; }
            public string Name { get; set; }
            public Position Position { get; set; }
            public int? Constant { get; set; }
            public bool IsConstant => !IsParent && Id == null && Constant != null;
        }

        private struct Margin
        {
            public int? Right { get; set; }
            public int? Left { get; set; }
            public int? Top { get; set; }
            public int? Bottom { get; set; }
        }

        private class Rule
        {
            public Rule(View view)
            {
                View = view;
            }

            public View View { get; }
            public LayoutRules? LayoutRule { get; private set; }
            public int? RelativeToViewId { get; private set; }
            public Margin Margin { get; private set; }
            public int? Width { get; private set; }
            public int? Height { get; private set; }

            private void SetMargin(LeftExpression leftExpression, RightExpression rightExpression)
            {
                Margin = GetMargin(leftExpression, rightExpression);
            }

            private Margin GetMargin(LeftExpression leftExpression, RightExpression rightExpression)
            {
                if (rightExpression.Constant == null) return new Margin();
                // assumes all constants are in Dp
                var constantPx = DpToPx(leftExpression.View.Context, rightExpression.Constant.Value);

                switch (leftExpression.Position)
                {
                    case Position.Top:
                        return new Margin { Top = constantPx };
                    case Position.Baseline:
                        return new Margin { Top = constantPx };
                    case Position.Right:
                        return new Margin { Right = -constantPx };
                    case Position.Bottom:
                        return new Margin { Bottom = -constantPx };
                    case Position.Left:
                        return new Margin { Left = constantPx };
                    case Position.Width:
                        return new Margin();
                    case Position.Height:
                        return new Margin();
                    case Position.CenterX:
                        return constantPx > 0 ?
                            new Margin { Left = constantPx } :
                            new Margin() { Right = -constantPx };
                    default:
                        throw new ArgumentException($"Constant expressions with {rightExpression.Position} are currently unsupported.");
                }
            }

            private void SetLayoutRule(LeftExpression leftExpression, RightExpression rightExpression)
            {
                LayoutRule = GetLayoutRule(leftExpression, rightExpression);
            }

            private static LayoutRules? GetLayoutRule(LeftExpression leftExpression, RightExpression rightExpression)
            {
                if (rightExpression.IsConstant)
                {
                    return null;
                }
                if (rightExpression.IsParent)
                {
                    return GetLayoutRuleForParent(leftExpression.Position, rightExpression.Position, leftExpression.Name);
                }
                return GetLayoutRuleForSibling(leftExpression.Position, rightExpression.Position,
                    leftExpression.Name, rightExpression.Name);
            }

            private static LayoutRules GetLayoutRuleForSibling(Position leftPosition, Position rightPosition, string leftExpressionName, string rightExpressionName)
            {
                if (leftPosition == Position.Bottom && rightPosition == Position.Bottom)
                    return LayoutRules.AlignBottom;
                if (leftPosition == Position.Top && rightPosition == Position.Top)
                    return LayoutRules.AlignTop;
                if (leftPosition == Position.Right && rightPosition == Position.Right)
                    return LayoutRules.AlignRight;
                if (leftPosition == Position.Left && rightPosition == Position.Left)
                    return LayoutRules.AlignLeft;
                if (leftPosition == Position.Top && rightPosition == Position.Bottom)
                    return LayoutRules.Below;
                if (leftPosition == Position.Bottom && rightPosition == Position.Top)
                    return LayoutRules.Above;
                if (leftPosition == Position.Left && rightPosition == Position.Right)
                    return LayoutRules.RightOf;
                if (leftPosition == Position.Right && rightPosition == Position.Left)
                    return LayoutRules.LeftOf;
                if (leftPosition == Position.Baseline && rightPosition == Position.Baseline)
                    return LayoutRules.AlignBaseline;
                if (leftPosition == Position.Width && rightPosition == Position.Width)
                    throw new ArgumentException("Unfortunately Android's relative layout isn't sophisticated enough to allow constraining the width of one view to the width of another.  Maybe try extracting several widths into a shared constant.");
                if (leftPosition == Position.Height && rightPosition == Position.Height)
                    throw new ArgumentException("Unfortunately Android's relative layout isn't sophisticated enough to allow constraining the height of one view to the height of another.  Maybe try extracting several heights into a shared constant.");
                throw new ArgumentException($"Unsupported relative positioning combination: {leftExpressionName}.{leftPosition} with {rightExpressionName}.{rightPosition}");
            }

            private static int DpToPx(Context ctx, float dp)
            {
                var displayMetrics = ctx.Resources.DisplayMetrics;
                return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, displayMetrics);
            }

            private static LayoutRules? GetLayoutRuleForParent(Position childPosition, Position parentPosition, string childName)
            {
                if (childPosition == Position.Top && parentPosition == Position.Top)
                    return LayoutRules.AlignParentTop;
                if (childPosition == Position.Right && parentPosition == Position.Right)
                    return LayoutRules.AlignParentRight;
                if (childPosition == Position.Bottom && parentPosition == Position.Bottom)
                    return LayoutRules.AlignParentBottom;
                if (childPosition == Position.Left && parentPosition == Position.Left)
                    return LayoutRules.AlignParentLeft;
                if (childPosition == Position.CenterX && parentPosition == Position.CenterX)
                    return LayoutRules.CenterHorizontal;
                if (childPosition == Position.CenterY && parentPosition == Position.CenterY)
                    return LayoutRules.CenterVertical;
                if (childPosition == Position.Center && parentPosition == Position.Center)
                    return LayoutRules.CenterInParent;
                if (childPosition == Position.Width && parentPosition == Position.Width)
                    return null;
                if (childPosition == Position.Height && parentPosition == Position.Height)
                    return null;
                throw new Exception($"Unsupported parent positioning combination: {childName}.{childPosition} with parent.{parentPosition}");
            }

            private void SetHeightWidth(LeftExpression leftExpression, RightExpression rightExpression)
            {
                if (leftExpression.Position == Position.Width && rightExpression.IsConstant && rightExpression.Constant.HasValue)
                    Width = DpToPx(leftExpression.View.Context, rightExpression.Constant.Value);
                if (leftExpression.Position == Position.Height && rightExpression.IsConstant && rightExpression.Constant.HasValue)
                    Height = DpToPx(leftExpression.View.Context, rightExpression.Constant.Value);
                if (leftExpression.Position == Position.Width && rightExpression.IsParent && rightExpression.Constant == null)
                    Width = ViewGroup.LayoutParams.MatchParent;
                if (leftExpression.Position == Position.Height && rightExpression.IsParent && rightExpression.Constant == null)
                    Height = ViewGroup.LayoutParams.MatchParent;
            }

            public void Initialize(LeftExpression leftExpression, RightExpression rightExpression)
            {
                if (!rightExpression.IsParent)
                {
                    RelativeToViewId = rightExpression.Id;
                }
                SetMargin(leftExpression, rightExpression);
                SetLayoutRule(leftExpression, rightExpression);
                SetHeightWidth(leftExpression, rightExpression);
            }
        }

        private static int _idCounter = 1;

        public static int GetCenterX(this View view)
        {
            return 0;
        }

        public static int GetCenterY(this View view)
        {
            return 0;
        }

        public static int GetCenter(this View view)
        {
            return 0;
        }

        public static int ToConst(this int i)
        {
            return i;
        }

        public static int ToConst(this float i)
        {
            return (int)i;
        }

        public static void ConstrainLayout(this RelativeLayout relativeLayout, Expression<Func<bool>> constraints)
        {
            _idCounter = 1;
            var constraintExpressions = FindConstraints(constraints.Body);
            var viewAndRule = ConvertConstraintsToRules(relativeLayout, constraintExpressions);
            UpdateLayoutParamsWithRules(viewAndRule);
        }

        private static void UpdateLayoutParamsWithRules(IEnumerable<Rule> viewAndRule)
        {
            var viewsGroupedByRule = viewAndRule.GroupBy(i => i.View);

            foreach (var viewAndRules in viewsGroupedByRule)
            {
                var view = viewAndRules.Key;
                var layoutParams = GetLayoutParamsOrAddDefault(view);

                foreach (var rule in viewAndRules)
                {
                    AddHeightWidthToLayoutParams(layoutParams, rule);
                    AddRuleToLayoutParams(layoutParams, rule);
                    AddMarginToLayoutParams(layoutParams, rule.Margin);
                }
            }
        }

        private static void AddHeightWidthToLayoutParams(RelativeLayout.LayoutParams layoutParams, Rule rule)
        {
            if (rule.Height != null)
                layoutParams.Height = rule.Height.Value;
            if (rule.Width != null)
                layoutParams.Width = rule.Width.Value;
        }

        private static IEnumerable<Rule> ConvertConstraintsToRules(RelativeLayout relativeLayout, List<BinaryExpression> constraintExpressions)
        {
            return constraintExpressions.Select(i => GetViewAndRule(i, relativeLayout));
        }

        private static void AddMarginToLayoutParams(RelativeLayout.LayoutParams layoutParams, Margin margin)
        {
            if (margin.Right.HasValue)
                layoutParams.RightMargin = margin.Right.Value;
            if (margin.Left.HasValue)
                layoutParams.LeftMargin = margin.Left.Value;
            if (margin.Top.HasValue)
                layoutParams.TopMargin = margin.Top.Value;
            if (margin.Bottom.HasValue)
                layoutParams.BottomMargin = margin.Bottom.Value;
        }

        private static void AddRuleToLayoutParams(RelativeLayout.LayoutParams layoutParams, Rule rule)
        {
            if (rule.LayoutRule == null) return;
            if (rule.RelativeToViewId.HasValue)
                layoutParams.AddRule(rule.LayoutRule.Value, rule.RelativeToViewId.Value);
            else
                layoutParams.AddRule(rule.LayoutRule.Value);
        }

        private static RelativeLayout.LayoutParams GetLayoutParamsOrAddDefault(View view)
        {
            if (view.LayoutParameters == null)
            {
                view.LayoutParameters = new RelativeLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent);
            }
            else
            {
                if (!(view.LayoutParameters is RelativeLayout.LayoutParams))
                {
                    throw new ArgumentException($"View #{view.Id} must have LayoutParameters set to either null or RelativeLayout.LayoutParams.");
                }
            }

            return (RelativeLayout.LayoutParams)view.LayoutParameters;
        }

        private static Rule GetViewAndRule(BinaryExpression expr, RelativeLayout relativeLayout)
        {
            var leftExpression = ParseLeftExpression(expr.Left);
            var rightExpression = ParseRightExpression(expr.Right, relativeLayout);
            return GetRule(leftExpression, rightExpression);
        }

        private static Rule GetRule(LeftExpression leftExpression, RightExpression rightExpression)
        {
            var rule = new Rule(leftExpression.View);
            rule.Initialize(leftExpression, rightExpression);
            return rule;
        }

        private static RightExpression ParseRightExpression(Expression expr, RelativeLayout relativeLayout)
        {
            Position? position = null;
            Expression memberExpression = null;
            int? constant = null;

            if (expr.NodeType == ExpressionType.Add || expr.NodeType == ExpressionType.Subtract)
            {
                var rb = (BinaryExpression)expr;
                if (IsConstant(rb.Left))
                {
                    throw new ArgumentException("Addition and substraction are only supported when there is a view on the left and a constant on the right");
                }
                if (IsConstant(rb.Right))
                {
                    constant = ConstantValue(rb.Right);
                    if (expr.NodeType == ExpressionType.Subtract)
                    {
                        constant = -constant;
                    }
                    expr = rb.Left;
                }
                else
                {
                    throw new NotSupportedException("Addition only supports constants: " + rb.Right.NodeType);
                }
            }

            if (IsConstant(expr))
            {
                position = Position.Constant;
                constant = ConstantValue(expr);
            }
            else
            {
                var fExpr = expr as MethodCallExpression;
                if (fExpr != null)
                {
                    position = GetPosition(fExpr);
                    memberExpression = fExpr.Arguments.FirstOrDefault() as MemberExpression;
                }
            }

            if (position == null)
            {
                var memExpr = expr as MemberExpression;
                if (memExpr == null)
                {
                    throw new NotSupportedException("Right hand side of a relation must be a member expression, instead it is " + expr);
                }

                position = GetPosition(memExpr);

                memberExpression = memExpr.Expression;
                if (memExpr.Expression == null)
                {
                    throw new NotSupportedException("Constraints should use views's Top, Bottom, etc properties, or extension methods like GetCenter().");
                }
            }

            View view = GetView(memberExpression);
            var memberName = GetName(memberExpression);
            var isParent = view == relativeLayout;

            if (view != null && !isParent && view.Id == -1)
            {
                view.Id = GenerateViewId();
            }

            return new RightExpression
            {
                IsParent = isParent,
                Id = view?.Id,
                Position = position.Value,
                Name = memberName,
                Constant = constant
            };
        }

        private static string GetName(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null) return memberExpression.Member.Name;
            return expression?.ToString();
        }

        private static View GetView(Expression viewExpr)
        {
            if (viewExpr == null) return null;
            var eval = Eval(viewExpr);
            var view = eval as View;
            if (view == null)
            {
                throw new NotSupportedException("Constraints only apply to views.");
            }
            return view;
        }

        private static int GenerateViewId()
        {
            // API level 17+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
            {
                return View.GenerateViewId();
            }

            return _idCounter++;
        }

        private static int ConstantValue(Expression expr)
        {
            return Convert.ToInt32(Eval(expr));
        }

        private static bool IsConstant(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Constant)
            {
                return true;
            }

            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var mexpr = (MemberExpression)expr;
                var m = mexpr.Member;
                if (m.MemberType == MemberTypes.Field)
                {
                    return true;
                }

                return false;
            }

            if (expr.NodeType == ExpressionType.Convert)
            {
                var cexpr = (UnaryExpression)expr;
                return IsConstant(cexpr.Operand);
            }

            var methodCall = expr as MethodCallExpression;
            if (methodCall != null)
            {
                return methodCall.Method.Name == nameof(ToConst);
            }

            return false;
        }

        private static LeftExpression ParseLeftExpression(Expression expr)
        {
            Position? position = null;
            MemberExpression viewExpr = null;

            var fExpr = expr as MethodCallExpression;
            if (fExpr != null)
            {
                position = GetPosition(fExpr);
                viewExpr = fExpr.Arguments.FirstOrDefault() as MemberExpression;
            }

            if (position == null)
            {
                var memExpr = expr as MemberExpression;
                if (memExpr == null)
                {
                    throw new NotSupportedException("Left hand side of a relation must be a member expression, instead it is " + expr);
                }

                position = GetPosition(memExpr);

                viewExpr = memExpr.Expression as MemberExpression;
            }

            if (viewExpr == null)
            {
                throw new NotSupportedException("Constraints should use views's Top, Bottom, etc properties, or extension methods like GetCenter().");
            }

            var eval = Eval(viewExpr);
            var view = eval as View;
            if (view == null)
            {
                throw new NotSupportedException("Constraints only apply to views.");
            }

            return new LeftExpression
            {
                View = view,
                Position = position.Value,
                Name = viewExpr.Member.Name
            };
        }

        private static Position GetPosition(MemberExpression memExpr)
        {
            switch (memExpr.Member.Name)
            {
                case nameof(View.Left):
                    return Position.Left;
                case nameof(View.Top):
                    return Position.Top;
                case nameof(View.Right):
                    return Position.Right;
                case nameof(View.Bottom):
                    return Position.Bottom;
                case nameof(View.Width):
                    return Position.Width;
                case nameof(View.Height):
                    return Position.Height;
                case nameof(View.Baseline):
                    return Position.Baseline;
                default:
                    throw new NotSupportedException("Property " + memExpr.Member.Name + " is not recognized.");
            }
        }

        private static Position GetPosition(MethodCallExpression fExpr)
        {
            switch (fExpr.Method.Name)
            {
                case nameof(GetCenterX):
                    return Position.CenterX;
                case nameof(GetCenterY):
                    return Position.CenterY;
                case nameof(GetCenter):
                    return Position.Center;
                default:
                    throw new NotSupportedException("Method " + fExpr.Method.Name + " is not recognized.");
            }
        }

        private static object Eval(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expr).Value;
            }

            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var mexpr = (MemberExpression)expr;
                var m = mexpr.Member;
                if (m.MemberType == MemberTypes.Field)
                {
                    var f = (FieldInfo)m;
                    var v = f.GetValue(Eval(mexpr.Expression));
                    return v;
                }
            }

            if (expr.NodeType == ExpressionType.Convert)
            {
                var cexpr = (UnaryExpression)expr;
                var op = Eval(cexpr.Operand);
                if (cexpr.Method != null)
                {
                    return cexpr.Method.Invoke(null, new[] { op });
                }
                else
                {
                    return Convert.ChangeType(op, cexpr.Type);
                }
            }

            return Expression.Lambda(expr).Compile().DynamicInvoke();
        }

        private static List<BinaryExpression> FindConstraints(Expression expr)
        {
            var binaryExpressions = new List<BinaryExpression>();
            FindConstraints(expr, binaryExpressions);
            return binaryExpressions;
        }

        private static void FindConstraints(Expression expr, List<BinaryExpression> constraintExprs)
        {
            var b = expr as BinaryExpression;
            if (b == null)
            {
                return;
            }

            if (b.NodeType == ExpressionType.AndAlso)
            {
                FindConstraints(b.Left, constraintExprs);
                FindConstraints(b.Right, constraintExprs);
            }
            else
            {
                constraintExprs.Add(b);
            }
        }
    }
}