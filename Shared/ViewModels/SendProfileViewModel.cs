﻿using System;

namespace Abeer.Shared.ViewModels
{
    public class SendProfileViewModel
    {
        public string UserId { get; set; }
        public string SendToId { get; set; }
        public string TargetUrl { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class SendAdViewModel
    {
        public string UserId { get; set; }
        public string SendToId { get; set; }
        public string TargetUrl { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Guid AdId { get; set; }
    }
}
