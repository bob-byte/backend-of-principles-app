using SET.Shared.Models;

using System;
using System.Linq;

namespace BusinessLogic;

public class ServiceOfHabit : IServiceOfHabit
{
    public bool ShouldHabitBeFollowed( UserHabit habit, ProgressOfHabit progress, int? countOfFollowedPerSpecificInterval )
    {
        #region Check parameters
        ArgumentNullException.ThrowIfNull( progress, nameof( progress ) );
        ArgumentNullException.ThrowIfNull( habit, nameof( habit ) );

        if (habit.Frequency == null)
        {
            throw new ArgumentException( "Frequency of habit is null", nameof( habit ) );
        }

        if (habit.Progresses == null)
        {
            throw new ArgumentException( "Progresses of habit is null", nameof( habit ) );
        }
        #endregion

        Frequency frequency = habit.Frequency;

        bool shouldHabitBeFollowed;
        int goalOfFollowedCount = frequency.Repeats;
        var zeroTime = TimeOnly.FromTimeSpan( TimeSpan.Zero );
        var dateTimeOfProgress = progress.Date.ToDateTime( zeroTime );

        switch (frequency.IntervalType())
        {
            case IntervalType.Day:
                {
                    shouldHabitBeFollowed = true;
                    break;
                }

            case IntervalType.Week:
                {
                    int daysToStartOfWeek = dateTimeOfProgress.DayOfWeek == DayOfWeek.Sunday
                        ? 6
                        : dateTimeOfProgress.DayOfWeek - DayOfWeek.Monday;

                    var firstDateOfCurrentWeek = DateOnly.FromDateTime( dateTimeOfProgress.AddDays( -daysToStartOfWeek ) );

                    int daysUntilEndOfWeek = dateTimeOfProgress.DayOfWeek == DayOfWeek.Sunday
                        ? 0
                        : (7 - (int)dateTimeOfProgress.DayOfWeek);

                    var lastDayOfCurrentWeek = DateOnly.FromDateTime( dateTimeOfProgress.AddDays( daysUntilEndOfWeek ) );

                    int followedCountThisWeek = habit.Progresses.Count( p => p.IsCompleted && firstDateOfCurrentWeek <= p.Date && p.Date <= lastDayOfCurrentWeek );

                    //including date of progress, so "+ 1"
                    int leftChances = daysUntilEndOfWeek + 1;
                    shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountThisWeek;
                    break;
                }

            case IntervalType.Month:
                {
                    DateTime firstDayOfMonth = new( dateTimeOfProgress.Year, dateTimeOfProgress.Month, 1 );
                    var firstDayOnlyOfMonth = DateOnly.FromDateTime( firstDayOfMonth );

                    DateTime lastDayOfMonth = firstDayOfMonth.AddMonths( 1 ).AddDays( -1 );
                    var lastDayOnlyOfMonth = DateOnly.FromDateTime( lastDayOfMonth );

                    int followedCountThisMonth = habit.Progresses.Count( p => p.IsCompleted && firstDayOnlyOfMonth <= p.Date && p.Date <= lastDayOnlyOfMonth );

                    int daysUntilEndOfMonth = (lastDayOfMonth - dateTimeOfProgress).Days;

                    //including date of progress, so "+ 1"
                    int leftChances = daysUntilEndOfMonth + 1;
                    shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountThisMonth;

                    break;
                }

            default:
                {
                    if (frequency.IntervalType() == IntervalType.Year)
                    {
                        int year = progress.Date.Year;

                        DateTime firstDateOfYear = new( year, month: 1, day: 1 );
                        var firstDateOfYearAsDateOnly = DateOnly.FromDateTime( firstDateOfYear );

                        DateTime earliestDateTime = habit.Progresses.Select( p => p.Date ).Min().ToDateTime( zeroTime );
                        DateTime largestDateTime = habit.Progresses.Select( p => p.Date ).Max().ToDateTime( zeroTime );

                        int diffOfEarliestAndLargest = (largestDateTime - earliestDateTime).Days;
                        int diffOfDateAndYearStart = (dateTimeOfProgress - firstDateOfYear).Days;

                        DateTime lastDayOfInterval = new( year + 1, 1, 1 );
                        lastDayOfInterval = lastDayOfInterval.AddDays( -1 );
                        var lastDayOfIntervalAsDateOnly = DateOnly.FromDateTime( lastDayOfInterval );

                        int daysUntilEndOfInterval = (lastDayOfInterval - dateTimeOfProgress).Days;
                        int followedCountWithinInterval;

                        if (diffOfEarliestAndLargest >= diffOfDateAndYearStart)
                        {
                            followedCountWithinInterval = habit.Progresses.Count( p => p.IsCompleted && firstDateOfYearAsDateOnly <= p.Date && p.Date <= lastDayOfIntervalAsDateOnly );
                        }
                        else
                        {
                            if (countOfFollowedPerSpecificInterval == null)
                            {
                                throw new InvalidOperationException( $"{nameof( countOfFollowedPerSpecificInterval )} cannot be null" );
                            }

                            followedCountWithinInterval = (int)countOfFollowedPerSpecificInterval;
                        }

                        //including date of progress, so "+ 1"
                        int leftChances = daysUntilEndOfInterval + 1;
                        shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountWithinInterval;
                    }
                    else if (frequency.IntervalType() == IntervalType.Other)
                    {
                        ProgressOfHabit? firstFollowedProgress = habit.Progresses.
                            Where( p => p.IsCompleted ).
                            OrderBy( p => p.Date ).
                            FirstOrDefault();
                        if (firstFollowedProgress == null || progress.Date < firstFollowedProgress.Date)
                        {
                            shouldHabitBeFollowed = true;
                        }
                        else
                        {
                            DateOnly lastDayOfInterval = default;

                            int dayInterval = frequency.IntervalLengthInDays;

                            DateOnly date = firstFollowedProgress.Date;
                            while (lastDayOfInterval == default)
                            {
                                date = date.AddDays( dayInterval );
                                if (date > progress.Date)
                                {
                                    lastDayOfInterval = date.AddDays( -1 );
                                }
                            }

                            DateOnly firstDayOfInterval = lastDayOfInterval.AddDays( -dayInterval + 1 );
                            int followedCountWithinInterval = habit.Progresses.Count( p => p.IsCompleted && firstDayOfInterval <= p.Date && p.Date <= lastDayOfInterval );
                            int daysUntilEndOfInterval = (lastDayOfInterval.ToDateTime( zeroTime ) - dateTimeOfProgress).Days;

                            //including date of progress, so "+ 1"
                            int leftChances = daysUntilEndOfInterval + 1;
                            shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountWithinInterval;
                        }
                    }
                    else
                    {
                        throw new ArgumentException( $"Invalid value of {nameof( ProgressOfHabit )}.{nameof( ProgressOfHabit.Habit )}.{nameof( UserHabit.Frequency )}.{nameof( Frequency.IntervalType )} in ServiceOfHabit.ShouldHabitBeFollowed" );
                    }

                    break;
                }
        }

        bool result = shouldHabitBeFollowed;
        return result;
    }
}

