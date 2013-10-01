using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class ProjectItemExtensions
    {
        public static bool TryGetPropertyValue<TResult>(this ProjectItem source, string propertyName,
                                                        out TResult propertyValue)
        {
            propertyValue = default(TResult);
            var property = source.Properties.AsEnumerable().SingleOrDefault(s => s.Name == propertyName);
            if (property == null)
            {
                return false;
            }
            propertyValue = (TResult) property.Value;
            return true;
        }


        public static TResult GetPropertyValue<TResult>(this ProjectItem source, string propertyName)
        {
            var propertyValue =
                source.Properties.AsEnumerable().Where(s => s.Name == propertyName).Select(s => s.Value).Single();
            return (TResult) propertyValue;
        }

        public static string GetFullPath(this ProjectItem source)
        {
            const string fullPath = "FullPath";
            var propertyValue = source.GetPropertyValue<string>(fullPath);
            return propertyValue;
        }

        public static bool IsLink(this ProjectItem source)
        {
            // is link file?
            bool isLink;
            return (source.TryGetPropertyValue("IsLink", out isLink) && isLink);
        }

        public static bool IsHavingProperties(this ProjectItem source, Predicate<IEnumerable<Property>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            var isHavingProperties = predicate(source.Properties.AsEnumerable());
            return isHavingProperties;
        }

        public static bool IsHavingProperties(this ProjectItem source, Predicate<Properties> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            var isHavingProperties = predicate(source.Properties);
            return isHavingProperties;
        }

        public static ProjectItem GetProjectItemContainingFullPath(this ProjectItem source, bool isLink = false)
        {
            // get target app.config full path
            var projectItemFullPath = source.GetFullPath();
            var dte = (DTE) source.DTE;
            return
                dte.GetProjectItemHavingProperties(properties =>
                                                   properties.GetFullPath() == projectItemFullPath &&
                                                   properties.GetIsLink() == isLink);

        }

        /// <summary>
        /// Removes all leading occurrences of second characters specified from the current System.String object.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string TrimStart(this string first, string second, string separator)
        {
            var firstSplit = first.Split(new []{separator}, StringSplitOptions.None);
            var secondSplit = second.Split(new[] { separator }, StringSplitOptions.None);

            var firstNewList = firstSplit.AsEnumerable();
            foreach (var s in secondSplit)
            {
                var chunk = firstNewList.First();
                if (s == chunk)
                {
                    firstNewList = firstNewList.Skip(1);
                    continue;
                }
                break;
            }

            // flatten list and add removed separator
            var firstTrim = firstNewList.Aggregate("", (s, s1) => string.Format(@"{0}{1}{2}", s, separator, s1));
            //var relativePath = sourceNewList.Aggregate(@"..", (s, s1) => string.Format(@"{0}\{1}", s, s1));

            return firstTrim;
        }

        public static string RelativePath(this string firstPath, string secondPath)
        {
            var trimStart = firstPath.TrimStart(secondPath, @"\");
            var relativePath = string.Format("{0}{1}", "..", trimStart);

            return relativePath;
        }

        public static string RelativeDirectory(this string source)
        {
            var lastIndex = source.LastIndexOf(@"\");
            var relativeDirectory = source.Substring(0, lastIndex + 1);

            return relativeDirectory;
        }

        public static string GetRelativePath(this ProjectItem source)
        {
            var filePath = source.GetFullPath();
            var projectContainingFile = source.ContainingProject.FullName;
            var relativePath = filePath.RelativePath(projectContainingFile);

            return relativePath;
        }

        public static string GetRelativeDirectory(this ProjectItem source)
        {
            var relativePath = source.GetRelativePath();
            var relativeDirectory = relativePath.RelativeDirectory();

            return relativeDirectory;
        }
    }
}