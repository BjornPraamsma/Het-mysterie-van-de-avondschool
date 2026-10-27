using System;
using System.Collections.Generic;
using System.Text;


namespace Game.Models
{
    public class BookcaseData
    {
        public int TileId { get; set; }
        public string Titel { get; set; } = "";
        public string Beschrijving { get; set; } = "";
        public string Vraag { get; set; } = "";
        public string Antwoord { get; set; } = "";
        public string Aanwijzing { get; set; } = "";
        public bool IsOpgelost { get; set; }
    }
}
