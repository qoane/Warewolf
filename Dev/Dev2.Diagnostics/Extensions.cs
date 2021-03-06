/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Diagnostics
{
    public static class Extensions
    {
        public static bool ContainsSafe(this string s, string filter)
        {
            if(string.IsNullOrEmpty(filter))
            {
                return true;
            }
            if(!string.IsNullOrEmpty(s))
            {
                return s.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;
            }
            return false;
        }

    }
}
