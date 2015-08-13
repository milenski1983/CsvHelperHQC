// Copyright 2009-2015 Josh Close and Contributors
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// http://csvhelper.com
namespace CsvHelper.Configuration
{
    using System.Reflection;

    /// <summary>
    ///     The configuration data for the reference map.
    /// </summary>
    public class CsvPropertyReferenceMapData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvPropertyReferenceMapData" /> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="mapping">The mapping this is a reference for.</param>
        public CsvPropertyReferenceMapData(PropertyInfo property, CsvClassMap mapping)
        {
            this.Property = property;
            this.Mapping = mapping;
        }

        /// <summary>
        ///     Gets or sets the header prefix to use.
        /// </summary>
        public virtual string Prefix
        {
            get
            {
                return this.prefix;
            }

            set
            {
                this.prefix = value;
                foreach (var propertyMap in this.Mapping.PropertyMaps)
                {
                    propertyMap.Data.Names.Prefix = value;
                }
            }
        }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo" /> that the data
        ///     is associated with.
        /// </summary>
        public virtual PropertyInfo Property { get; private set; }

        /// <summary>
        ///     Gets the mapping this is a reference for.
        /// </summary>
        public CsvClassMap Mapping { get; private set; }

        private string prefix;
    }
}