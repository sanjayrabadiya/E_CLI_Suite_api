using System;
using System.Linq;
using System.Linq.Expressions;

namespace GSC.Helper
{
    public class GenericSpecification<T>
    {
        private readonly Expression<Func<T, bool>> _expression;

        public GenericSpecification(Expression<Func<T, bool>> expression)
        {
            _expression = expression;
        }

        public bool IsSatifiedBy(T entity)
        {
            return _expression.Compile().Invoke(entity);
        }
    }

    internal sealed class IdentificationSpecification<T> : Specification<T>
    {
        public override Expression<Func<T, bool>> ExpressionTo()
        {
            return x => true;
        }
    }

    public abstract class Specification<T>
    {
        public static readonly Specification<T> All = new IdentificationSpecification<T>();

        public bool IsSatified(T entity)
        {
            var predicate = ExpressionTo().Compile();
            return predicate.Invoke(entity);
        }

        public abstract Expression<Func<T, bool>> ExpressionTo();

        public Specification<T> And(Specification<T> specification)
        {
            if (this == All)
                return specification;
            if (specification == All)
                return this;
            return new AndSpecification<T>(this, specification);
        }

        public Specification<T> Or(Specification<T> specification)
        {
            if (this == All || specification == All)
                return All;
            return new OrSpecification<T>(this, specification);
        }

        public Specification<T> Not(Specification<T> specification)
        {
            return new NotSpecification<T>(this);
        }
    }

    internal sealed class AndSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public AndSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ExpressionTo()
        {
            var leftPredicate = _left.ExpressionTo();
            var rightPredicate = _right.ExpressionTo();

            var andExpression = Expression.AndAlso(leftPredicate.Body, rightPredicate.Body);
            return Expression.Lambda<Func<T, bool>>(andExpression, leftPredicate.Parameters.Single());
        }
    }

    internal sealed class OrSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ExpressionTo()
        {
            var leftPredicate = _left.ExpressionTo();
            var rightPredicate = _right.ExpressionTo();

            var expression = Expression.OrElse(leftPredicate.Body, rightPredicate.Body);

            return Expression.Lambda<Func<T, bool>>(expression, leftPredicate.Parameters.Single());
        }
    }

    internal sealed class NotSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _specification;

        public NotSpecification(Specification<T> specification)
        {
            _specification = specification;
        }

        public override Expression<Func<T, bool>> ExpressionTo()
        {
            var predicate = _specification.ExpressionTo();

            var expression = Expression.Not(predicate.Body);
            return Expression.Lambda<Func<T, bool>>(expression, predicate.Parameters.Single());
        }
    }
}