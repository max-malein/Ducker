using System;
using System.Collections.Generic;
using System.Linq;
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
            StringBuilder contentsHeaderBuilder = new StringBuilder();
            StringBuilder contentsSplitterBuilder = new StringBuilder();
            StringBuilder contentsIconsBuilder = new StringBuilder();

            contentsHeaderBuilder.Append("|");
            contentsSplitterBuilder.Append("|");
            contentsIconsBuilder.Append("|");

            var compsBySubCategory = components
                .GroupBy(c => c.SubCategory)
                .OrderBy(g => g.Key)
                .ThenBy(g => g.Select(c => Enum.Parse(typeof(GH_Exposure), c.Exposure)))
                .ThenBy(g => g.Select(c => c.Name));

            foreach (var subCategoryComponents in compsBySubCategory)
            {
                // Skip this subcategory if there's no visible components
                if (settings.IgnoreHidden && subCategoryComponents.All(c => c.Exposure == "hidden"))
                {
                    continue;
                }

                contentsHeaderBuilder.Append(" " + subCategoryComponents.Key + " |");
                contentsSplitterBuilder.Append(" --- |");

                builder.AppendLine(Header(subCategoryComponents.Key));

                foreach (var component in subCategoryComponents)
                {
                    if (component.Exposure == "hidden" && settings.IgnoreHidden)
                        continue;

                    var componentIcon = Image("", docContent.RelativePathIcons, component.GetNameWithoutSpaces());
                    contentsIconsBuilder.Append(" " + LinkToSection(component.Name, componentIcon));

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

                contentsIconsBuilder.Append(" |");
            }

            docContent.Document = string.Join("\n", contentsHeaderBuilder.ToString(),
                contentsSplitterBuilder.ToString(),
                contentsIconsBuilder.ToString(),
                builder.ToString());

            docContent.Icons = base.ReadIcons(components);
            return docContent;
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