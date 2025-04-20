// Matches the C# Summary model in MessageAggregator/Domain/Models/Summary.cs
export interface Summary {
  id: string; // Guid translates to string in JSON
  chatName: string | null; // Nullable string
  description: string; // Renamed from content
  categoryName: string; // Renamed from category
  createdAt: string; // DateTime translates to string (ISO format)
}

// Placeholder for ChatBase if needed later
export interface ChatBase {
  // Define properties based on TL.ChatBase if required
  id: number;
  title: string;
}

// Define other DTOs or models as needed
export interface AnalyzeRequestData {
    Data: string[];
    ChatName: string;
}
