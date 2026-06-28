namespace Charity_Kousar_Donation.DTOs;

public record AiOptimizeRequest(
    string Text,
    string Language,
    string FieldType,
    string? CampaignTitle,
    string? Context);

public record AiOptimizeResponse(
    string Optimized,
    string? Alternative,
    string? Tips);

public record AiTranslateRequest(
    string Text,
    string From,
    string To);

public record AiTranslateResponse(string Translated);
