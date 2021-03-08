// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public LinQueryable<T> Select(Expression<Func<T, object>> predicate)
        {
            string query = Data.Select(predicate).ToString();
            Query += query[(query.LastIndexOf("]") + 1)..];

            return this;
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
    }
}
