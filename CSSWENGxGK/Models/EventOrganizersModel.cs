using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSSWENGxGK.Models
{
    public class EventOrganizers
    {
        public Event Event { get; set; }
        public List<Organizer> Organizers { get; set; }
    }
}