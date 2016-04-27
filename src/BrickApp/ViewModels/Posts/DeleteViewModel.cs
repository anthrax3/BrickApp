﻿using System;
using System.Collections.Generic;
using BrickApp.Models.BrickStore;

namespace BrickApp.ViewModels.Posts
{
    public class DeleteViewModel
    {
        public IEnumerable<Category> TopLevelCategories { get; set; }
        public List<WebsiteAd> CurrentAds { get; set; }
    }
}