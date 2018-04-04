using System; 
using System.Collections.Generic; 
using System.ComponentModel; 
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;
//
using Microsoft.AspNetCore.Identity;

namespace auth.Data { 

    public class ApplicationUser : IdentityUser { 

        #region Constructor 

        public ApplicationUser() { } 

        #endregion 

        #region Properties 
        
        public string DisplayName { get; set; } 
        
        public string Notes { get; set; } 
        
        [Required] 
        public int Type { get; set; } 
        
        [Required] 
        public int Flags { get; set; } 
        
        [Required] 
        public DateTime CreatedDate { get; set; } 
        
        [Required] 
        public DateTime LastModifiedDate { get; set; } 
        
        #endregion 

        #region Lazy-Load Properties 

        /// <summary>
        /// A list of all the refresh tokens issued for this user.!--
        /// </summary>
        public virtual List<Token> Tokens { get; set; }

        #endregion

    } 
}

