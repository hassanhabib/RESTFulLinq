// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using RESTFulSense.Clients;

namespace RESTFulLinq.Clients
{
    public class LinQueryable<T>
    {
        public IRESTFulApiClient Client { get; set; }
        public string RelativeUrl { get; set; }
        public string Query { get; set; }

        public IQueryable<T> Data { get; set; } =
            Enumerable.Empty<T>().AsQueryable();

        public LinQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            string query = Data.Where(predicate).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
        }

        public LinQueryable<TResult> Select<TResult>(Expression<Func<T, TResult>> predicate, params Type[] localTypes)
        {
            string query = Data.Select(predicate).ToString();
            query = query[(query.LastIndexOf("]") + 1)..];
            query = FixAnonymousSelect(query);
            if (localTypes?.Length > 0)
            {
                foreach (var type in localTypes)
                {
                    query = query.Replace($"new {type.Name}()", "new");
                }
            }

            if (typeof(T) == typeof(TResult))
            {
                Query += query;
                return this as LinQueryable<TResult>;
            }

            return new LinQueryable<TResult>
            {
                RelativeUrl = RelativeUrl,
                Client = Client,
                Query = $"{Query}{query}"
            };
        }

        public LinQueryable<T> FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            string query = Data.FirstOrDefault(predicate).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
        }

        public LinQueryable<T> Take(int count)
        {
            string query = Data.Take(count).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
        }

        public LinQueryable<T> Skip(int count)
        {
            string query = Data.Skip(count).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
        }

        public Task<List<T>> ToListAsync() => ToListAsync<T>();

        public async Task<List<TResult>> ToListAsync<TResult>()
        {
            string query = Query[(Query.IndexOf(".") + 1)..];
            query = CleanUpQuery(query);
            query = HttpUtility.UrlEncode(query);

            return await this.Client.GetContentAsync<List<TResult>>(
                $"{this.RelativeUrl}?linquery={query}");
        }

        public LinQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> expression)
        {
            string query = Data.OrderBy(expression).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
        }

        public LinQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> expression)
        {
            string query = Data.OrderByDescending(expression).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
        }

        public LinQueryable<T> OrderBy(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            string query = orderBy(Data).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
        }

        // helper functions
        private static string CleanUpQuery(string query)
        {
            return query
                .Replace("AndAlso", "&&")
                .Replace("OrElse", "||");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FixAnonymousSelect(string query)
        {
            while (true)
            {
                var match = AnonymousSelectRegex.Match(query);
                if (!match.Success)
                    break;

                var beginGroup = match.Groups[1];
                var selectGroup = match.Groups[3];
                var endGroup = match.Groups[4];

                query = $"{beginGroup}{{{selectGroup}}}{endGroup}";
            }

            return query;
        }

        private static readonly Regex AnonymousSelectRegex = new Regex(
            @"(.+)(<>f__AnonymousType[^\(]+)\(([^\)]+)\)(.+)",
            RegexOptions.Compiled
        );
    }
}
