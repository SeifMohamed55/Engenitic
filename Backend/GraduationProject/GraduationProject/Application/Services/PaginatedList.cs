﻿using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace GraduationProject.Application.Services
{

    public interface IPaginatedList : IList
    {
        int PageIndex { get; }
        int TotalCount { get; }
        int PageSize { get; }
        int TotalPages { get; }
    }

    public class PaginatedList<T> : List<T>, IPaginatedList
    {
        public int PageIndex { get; private set; } // starting from 1
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        private PaginatedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            AddRange(source);
        }

        public bool HasPreviousPage
        {
            get
            {
                return PageIndex > 0;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return PageIndex + 1 < TotalPages;
            }
        }

        public void Prepend(T item)
        {
            Insert(0, item);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize = 10)
        {
            var totalCount = await source.CountAsync();

            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, pageIndex, pageSize, totalCount);
        }

        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize = 10)
        {
            var totalCount = source.Count();

            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedList<T>(items, pageIndex, pageSize, totalCount);
        }
    }
}
