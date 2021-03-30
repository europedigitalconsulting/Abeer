using Abeer.Shared.ViewModels;
using AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Shared.Functional
{
    public class QrCode
    {
        public QrCode()
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Key { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
