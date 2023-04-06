using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Ducker.Core
{
    /// <summary>
    /// Generates a markdown with Emu style.
    /// </summary>
    public class StandardMdDocGenerator : MarkDownDocGenerator
    {
        /// <summary>
        /// Creates the contents of the document based on components and the export settings
        /// </summary>
        /// <param name="components">The components included in the gha.</param>
        /// <param name="settings">The output settings.</param>
        /// <returns>Content of the document.</returns>
        public override DocumentContent Create(List<DuckerComponent> components, ExportSettings settings)
        {
            DocumentContent docContent = new DocumentContent();
            StringBuilder builder = new StringBuilder();            

            var compsBySubCategory = components
                .OrderBy(c => c.SubCategory)
                .ThenBy(c => Enum.Parse(typeof(GH_Exposure), c.Exposure))
                .ThenBy(c => c.Name)
                .GroupBy(c => c.SubCategory)
                .ToDictionary(g => g.Key, g => g.ToList()); 

            if (settings.TableOfContents)
            {
                builder.AppendLine(GenerateTOC(compsBySubCategory, settings, docContent));
            }

            foreach (var key in compsBySubCategory.Keys)
            {
                var subCategoryComponents = compsBySubCategory[key];

                // Skip this subcategory if there are no visible components
                if (settings.IgnoreHidden && subCategoryComponents.All(c => c.Exposure == "hidden"))
                {
                    continue;
                }

                builder.AppendLine(Header(key));

                foreach (var component in subCategoryComponents)
                {
                    if (component.Exposure == "hidden" && settings.IgnoreHidden)
                        continue;

                    var componentIcon = Image("", docContent.RelativePathIcons, component.GetNameWithoutSpaces());

                    builder.AppendLine(Header(componentIcon + " " + component.Name, 2));
                    builder.Append(Paragraph(Bold(nameof(component.Name) + ":") + " " + component.Name));
                    builder.Append(Paragraph(Bold(nameof(component.NickName) + ":") + " " + component.NickName));
                    builder.Append(Paragraph(Bold(nameof(component.Description) + ":") + " " + component.Description));
                    builder.Append(Environment.NewLine);

                    if (component.Input.Count > 0)
                    {
                        builder.AppendLine(Header(nameof(component.Input), 3));
                        string table = GenerateParamTable(component.Input);
                        builder.Append(table);
                    }

                    if (component.Output.Count > 0)
                    {
                        builder.AppendLine(Header(nameof(component.Output), 3));
                        string table = GenerateParamTable(component.Output);
                        builder.Append(table);
                    }
                }
            }

            docContent.Document = builder.ToString();

            docContent.Icons = base.ReadIcons(components);
            return docContent;
        }

        private string GenerateTOC(Dictionary<string, List<DuckerComponent>> compsBySubCategory, ExportSettings settings, DocumentContent docContent)
        {
            StringBuilder contentsHeaderBuilder = new StringBuilder("\n|");
            StringBuilder contentsSplitterBuilder = new StringBuilder("|");
            StringBuilder contentsIconsBuilder = new StringBuilder("|");

            foreach (var subCategoryComponents in compsBySubCategory)
            {
                // Skip this subcategory if there are no visible components
                if (settings.IgnoreHidden && subCategoryComponents.Value.All(c => c.Exposure == "hidden"))
                {
                    continue;
                }

                contentsHeaderBuilder.Append(" " + subCategoryComponents.Key + " |");
                contentsSplitterBuilder.Append(" --- |");

                foreach (var component in subCategoryComponents.Value)
                {
                    if (component.Exposure == "hidden" && settings.IgnoreHidden)
                        continue;

                    var componentIcon = Image("", docContent.RelativePathIcons, component.GetNameWithoutSpaces());
                    contentsIconsBuilder.Append(" " + LinkToSection(component.Name, componentIcon, settings.GithubPages));
                }

                contentsIconsBuilder.Append(" |");
            }

            contentsIconsBuilder.AppendLine(); // line after the table

            var toc = string.Join("\n",
                contentsHeaderBuilder.ToString(),
                contentsSplitterBuilder.ToString(),
                contentsIconsBuilder.ToString());

            return toc;
        }

        private enum GH_Exposure
        {
            hidden = -1,
            primary = 2,
            secondary = 4,
            tertiary = 8,
            quarternary = 16,
            quinary = 32,
            senary = 64,
            septenary = 128,
            octonary = 256,
            last = 256,
            obscure = 65536,
        }
    }
}