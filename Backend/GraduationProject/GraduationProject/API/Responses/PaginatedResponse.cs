﻿using GraduationProject.Application.Services;
using System.Collections;

namespace GraduationProject.API.Responses
{
    public class PaginatedResponse
    {
        public int TotalPages { get; init; }
        public int TotalItems { get; init; }
        public string CurrentlyViewing { get; init; }
        public int PageIndex { get; init; }
        public IList PaginatedList { get; init; }

        public PaginatedResponse(IPaginatedList list)
        {
            PaginatedList = list;
            TotalPages = list.TotalPages;
            TotalItems = list.TotalCount;
            PageIndex = list.PageIndex;
            CurrentlyViewing =
                $"({1 + (list.PageIndex - 1) * list.PageSize}" +
                $" - " +
                $"{(list.PageIndex != list.TotalPages ? list.PageIndex * list.PageSize : (list.PageIndex - 1) * list.PageSize + list.Count)})";
        }
    }
}
