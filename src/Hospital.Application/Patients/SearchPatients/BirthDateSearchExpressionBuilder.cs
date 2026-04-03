using Hospital.Domain.Entities;
using System.Linq.Expressions;

namespace Hospital.Application.Patients.SearchPatients;

public static class BirthDateSearchExpressionBuilder
{
    public static IQueryable<Patient> Apply(IQueryable<Patient> query, BirthDateSearchValue searchValue)
    {
        return Apply(query, searchValue, DateTimeOffset.UtcNow);
    }

    public static IQueryable<Patient> Apply(
        IQueryable<Patient> query,
        BirthDateSearchValue searchValue,
        DateTimeOffset now)
    {
        return searchValue.Prefix switch
        {
            BirthDateSearchPrefix.Eq => query.Where(x => x.BirthDate >= searchValue.LowerBound && x.BirthDate < searchValue.UpperBound),
            BirthDateSearchPrefix.Ne => query.Where(x => x.BirthDate < searchValue.LowerBound || x.BirthDate >= searchValue.UpperBound),
            BirthDateSearchPrefix.Gt => query.Where(x => x.BirthDate >= searchValue.UpperBound),
            BirthDateSearchPrefix.Ge => query.Where(x => x.BirthDate >= searchValue.LowerBound),
            BirthDateSearchPrefix.Lt => query.Where(x => x.BirthDate < searchValue.LowerBound),
            BirthDateSearchPrefix.Le => query.Where(x => x.BirthDate < searchValue.UpperBound),
            BirthDateSearchPrefix.Sa => query.Where(x => x.BirthDate >= searchValue.UpperBound),
            BirthDateSearchPrefix.Eb => query.Where(x => x.BirthDate < searchValue.LowerBound),
            BirthDateSearchPrefix.Ap => ApplyApproximate(query, searchValue, now),
            _ => throw new ArgumentOutOfRangeException(nameof(searchValue.Prefix), searchValue.Prefix, "Unsupported birthDate search prefix.")
        };
    }

    public static IQueryable<Patient> Apply(IQueryable<Patient> query, BirthDateSearchGroup searchGroup)
    {
        return Apply(query, searchGroup, DateTimeOffset.UtcNow);
    }

    public static IQueryable<Patient> Apply(
        IQueryable<Patient> query,
        BirthDateSearchGroup searchGroup,
        DateTimeOffset now)
    {
        if (searchGroup.Values.Count == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(Patient), "patient");
        Expression? predicateBody = null;

        foreach (var searchValue in searchGroup.Values)
        {
            var itemPredicate = BuildPredicate(parameter, searchValue, now);
            predicateBody = predicateBody is null
                ? itemPredicate
                : Expression.OrElse(predicateBody, itemPredicate);
        }

        var lambda = Expression.Lambda<Func<Patient, bool>>(predicateBody!, parameter);
        return query.Where(lambda);
    }

    private static IQueryable<Patient> ApplyApproximate(
        IQueryable<Patient> query,
        BirthDateSearchValue searchValue,
        DateTimeOffset now)
    {
        var interval = searchValue.UpperBound - searchValue.LowerBound;
        var distanceFromNow = now - searchValue.LowerBound;
        var approximation = TimeSpan.FromTicks(
            Math.Max(
                interval.Ticks,
                (long)Math.Ceiling(Math.Abs(distanceFromNow.Ticks) * 0.1)));

        var lowerBound = searchValue.LowerBound - approximation;
        var upperBound = searchValue.UpperBound + approximation;

        return query.Where(x => x.BirthDate >= lowerBound && x.BirthDate < upperBound);
    }

    private static Expression BuildPredicate(
        ParameterExpression parameter,
        BirthDateSearchValue searchValue,
        DateTimeOffset now)
    {
        var birthDateProperty = Expression.Property(parameter, nameof(Patient.BirthDate));
        var lowerBound = Expression.Constant(searchValue.LowerBound);
        var upperBound = Expression.Constant(searchValue.UpperBound);

        return searchValue.Prefix switch
        {
            BirthDateSearchPrefix.Eq => Expression.AndAlso(
                Expression.GreaterThanOrEqual(birthDateProperty, lowerBound),
                Expression.LessThan(birthDateProperty, upperBound)),
            BirthDateSearchPrefix.Ne => Expression.OrElse(
                Expression.LessThan(birthDateProperty, lowerBound),
                Expression.GreaterThanOrEqual(birthDateProperty, upperBound)),
            BirthDateSearchPrefix.Gt => Expression.GreaterThanOrEqual(birthDateProperty, upperBound),
            BirthDateSearchPrefix.Ge => Expression.GreaterThanOrEqual(birthDateProperty, lowerBound),
            BirthDateSearchPrefix.Lt => Expression.LessThan(birthDateProperty, lowerBound),
            BirthDateSearchPrefix.Le => Expression.LessThan(birthDateProperty, upperBound),
            BirthDateSearchPrefix.Sa => Expression.GreaterThanOrEqual(birthDateProperty, upperBound),
            BirthDateSearchPrefix.Eb => Expression.LessThan(birthDateProperty, lowerBound),
            BirthDateSearchPrefix.Ap => BuildApproximatePredicate(birthDateProperty, searchValue, now),
            _ => throw new ArgumentOutOfRangeException(nameof(searchValue.Prefix), searchValue.Prefix, "Unsupported birthDate search prefix.")
        };
    }

    private static Expression BuildApproximatePredicate(
        MemberExpression birthDateProperty,
        BirthDateSearchValue searchValue,
        DateTimeOffset now)
    {
        var interval = searchValue.UpperBound - searchValue.LowerBound;
        var distanceFromNow = now - searchValue.LowerBound;
        var approximation = TimeSpan.FromTicks(
            Math.Max(
                interval.Ticks,
                (long)Math.Ceiling(Math.Abs(distanceFromNow.Ticks) * 0.1)));

        var lowerBound = Expression.Constant(searchValue.LowerBound - approximation);
        var upperBound = Expression.Constant(searchValue.UpperBound + approximation);

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(birthDateProperty, lowerBound),
            Expression.LessThan(birthDateProperty, upperBound));
    }
}
