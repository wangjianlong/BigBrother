﻿using LoowooTech.Land.Zhoushan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoowooTech.Land.Zhoushan.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> SetPage<T>(this IQueryable<T> query, PageParameter page)
        {
            if (page == null) return query;

            if (page.RecordCount == 0)
            {
                page.RecordCount = query.Count();
            }
            return query.Skip(page.PageSize * (page.PageIndex - 1)).Take(page.PageSize);
        }

        public static IEnumerable<T> SetPage<T>(this IEnumerable<T> query, PageParameter page)
        {
            if (page == null) return query;

            if (page.RecordCount == 0)
            {
                page.RecordCount = query.Count();
            }
            return query.Skip(page.PageSize * (page.PageIndex - 1)).Take(page.PageSize);
        }
    }
}
