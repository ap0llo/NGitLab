﻿namespace NGitLab.Models
{
    /// <summary>
    /// The access levels are defined in the Gitlab::Access module. Currently, these levels are recognized:
    /// </summary>
    /// <remarks>https://github.com/gitlabhq/gitlabhq/blob/master/doc/api/members.md</remarks>
    public enum AccessLevel
    {
        Guest = 10,
        Reporter = 20, 
        Developer = 30,
        Master = 40, 
        /// <summary>
        /// Only valid for groups.
        /// </summary>
        Owner = 50
    }
}