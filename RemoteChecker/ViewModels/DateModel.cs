using Microsoft.AspNetCore.Mvc;
using RemoteChecker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteChecker.ViewModels
{
    public class DateModel
    {
        public CheckRequest CheckRequest { get; set; }

        public IEnumerable<CheckHistory> CheckHistories;

        [BindProperty, DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; }

        [BindProperty, DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Stop { get; set; }
    }
}
