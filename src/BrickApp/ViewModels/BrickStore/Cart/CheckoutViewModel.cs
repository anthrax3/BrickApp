﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BrickApp.Models.Identity;
using BrickApp.Models.BrickStore;

namespace BrickApp.ViewModels.Cart
{
    public class CheckoutViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<UserAddress> UserAddresses { get; set; }

        [Display(Name ="Remember Address")]
        public bool RememberAddress { get; set; }
    }
}