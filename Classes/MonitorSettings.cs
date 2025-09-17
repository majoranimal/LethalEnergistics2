using System.Collections.Generic;

namespace LethalEnergistics2.Classes
{
    internal struct MonitorSettings
    {

        public int? hoveredOption;
        public int? displayStart;
        public int? displayEnd;
        public string? currentPage;

        public int? itemsPreviousSortLength;
        public int? itemsPreviousSortOrder;
        public int? itemsSortOrder;
        public List<int>? sortedItems;
        public List<int>? selectedItems;

        public int? summariesPreviousSortLength;
        public int? summariesPreviousSortOrder;
        public int? summariesSortOrder;
        public List<string>? sortedSummaries;
        public List<string>? selectedSummaries;

        public int? toolsPreviousSortLength;
        public List<int>? sortedTools;
        public List<int>? selectedTools;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public MonitorSettings()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }

        public void ApplyDefaults(MonitorSettings newSettings)
        {
            hoveredOption = newSettings.hoveredOption ?? hoveredOption;
            displayStart = newSettings.displayStart ?? displayStart;
            displayEnd = newSettings.displayEnd ?? displayEnd;
            currentPage = newSettings.currentPage ?? currentPage;

            itemsPreviousSortLength = newSettings.itemsPreviousSortLength ?? itemsPreviousSortLength;
            itemsPreviousSortOrder = newSettings.itemsPreviousSortOrder ?? itemsPreviousSortOrder;
            itemsSortOrder = newSettings.itemsSortOrder ?? itemsSortOrder;
            sortedItems = newSettings.sortedItems ?? sortedItems;
            selectedItems = newSettings.selectedItems ?? selectedItems;

            summariesPreviousSortLength = newSettings.summariesPreviousSortLength ?? summariesPreviousSortLength;
            summariesPreviousSortOrder = newSettings.summariesPreviousSortOrder ?? summariesPreviousSortOrder;
            summariesSortOrder = newSettings.summariesSortOrder ?? summariesSortOrder;
            sortedSummaries = newSettings.sortedSummaries ?? sortedSummaries;
            selectedSummaries = newSettings.selectedSummaries ?? selectedSummaries;

            toolsPreviousSortLength = newSettings.toolsPreviousSortLength ?? toolsPreviousSortLength;
            sortedTools = newSettings.sortedTools ?? sortedTools;
            selectedTools = newSettings.selectedTools ?? selectedTools;
        }
    }
}
