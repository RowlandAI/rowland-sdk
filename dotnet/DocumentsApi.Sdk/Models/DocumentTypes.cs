using System.Text.Json.Serialization;

namespace DocumentsApi.Sdk.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentType
    {
        [JsonPropertyName("affidavit")]
        Affidavit,

        [JsonPropertyName("amendment")]
        Amendment,

        [JsonPropertyName("assignment")]
        Assignment,

        [JsonPropertyName("authorization_for_expenditure")]
        AuthorizationForExpenditure,

        [JsonPropertyName("chain_of_title")]
        ChainOfTitle,

        [JsonPropertyName("communitization_agreement")]
        CommunitizationAgreement,

        [JsonPropertyName("confidentiality_agreement")]
        ConfidentialityAgreement,

        [JsonPropertyName("deed")]
        Deed,

        [JsonPropertyName("division_order")]
        DivisionOrder,

        [JsonPropertyName("easement")]
        Easement,

        [JsonPropertyName("farmout_agreement")]
        FarmoutAgreement,

        [JsonPropertyName("gas_gathering_agreement")]
        GasGatheringAgreement,

        [JsonPropertyName("gas_processing_agreement")]
        GasProcessingAgreement,

        [JsonPropertyName("joint_operating_agreement")]
        JointOperatingAgreement,

        [JsonPropertyName("lease")]
        Lease,

        [JsonPropertyName("letter_agreement")]
        LetterAgreement,

        [JsonPropertyName("letter_of_intent")]
        LetterOfIntent,

        [JsonPropertyName("lien")]
        Lien,

        [JsonPropertyName("marketing_agreement")]
        MarketingAgreement,

        [JsonPropertyName("mineral_deed")]
        MineralDeed,

        [JsonPropertyName("mortgage")]
        Mortgage,

        [JsonPropertyName("notice_of_extension")]
        NoticeOfExtension,

        [JsonPropertyName("participation_agreement")]
        ParticipationAgreement,

        [JsonPropertyName("pooling_agreement")]
        PoolingAgreement,

        [JsonPropertyName("production_sharing_agreement")]
        ProductionSharingAgreement,

        [JsonPropertyName("purchase_and_sale_agreement")]
        PurchaseAndSaleAgreement,

        [JsonPropertyName("ratification")]
        Ratification,

        [JsonPropertyName("regulatory_filing")]
        RegulatoryFiling,

        [JsonPropertyName("release")]
        Release,

        [JsonPropertyName("right_of_way")]
        RightOfWay,

        [JsonPropertyName("royalty_deed")]
        RoyaltyDeed,

        [JsonPropertyName("saltwater_disposal_agreement")]
        SaltwaterDisposalAgreement,

        [JsonPropertyName("subordination_agreement")]
        SubordinationAgreement,

        [JsonPropertyName("solar_lease")]
        SolarLease,

        [JsonPropertyName("stipulation")]
        Stipulation,

        [JsonPropertyName("surface_use_agreement")]
        SurfaceUseAgreement,

        [JsonPropertyName("title_report")]
        TitleReport,

        [JsonPropertyName("unitization_agreement")]
        UnitizationAgreement,

        [JsonPropertyName("well_proposal")]
        WellProposal,

        [JsonPropertyName("wind_lease")]
        WindLease,

        [JsonPropertyName("other")]
        Other
    }
}
