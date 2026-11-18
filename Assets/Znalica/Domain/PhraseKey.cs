namespace Znalica.Domain
{
    /// <summary>
    /// Every phrase the game shows or speaks: instructions, praise, explanations, spoken activity names and the
    /// screen-only texts. The Croatian text and the clip names for each key live in Hr.json, not in code.
    /// Later steps add their own keys. Append new keys at the end only: Main.unity serializes PhraseKey as an
    /// integer (the menu keys are 0, 1 and 2), so inserting or reordering silently changes what the scene refers to.
    /// </summary>
    public enum PhraseKey
    {
        // Activity menu
        ActivityNameNumberLine,
        ActivityNameComparison,
        ActivityNameArithmetic,

        // Screen-only texts (no voice)
        ButtonReplay,
        ButtonNext,
        GateQuestion,

        // Spoken feedback
        PraiseRight,
        ExplainJump,
        ThenMore,
        Is,

        // Number words used inside screen texts (text only, no voice): these forms do not change with gender
        NumberWord3,
        NumberWord4,
        NumberWord5,
        NumberWord6,
        NumberWord7,
        NumberWord8,
        NumberWord9,

        // Bare counting form of each number, spoken where a number stands alone (see PhraseSlot)
        Number00,
        Number01,
        Number02,
        Number03,
        Number04,
        Number05,
        Number06,
        Number07,
        Number08,
        Number09,
        Number10,
        Number11,
        Number12,
        Number13,
        Number14,
        Number15,
        Number16,
        Number17,
        Number18,
        Number19,
        Number20,

        // Step 3a: the jump back that explains a subtraction (placeholder wording, for the Croatian review)
        ExplainJumpBack,
        ThenLess,

        // Step 3b: the number pad and round-end texts (screen only). Wording: the pad's two come from the spec's Input
        // section; the round-end three are placeholders for the Croatian review.
        ButtonErase,
        ButtonOk,
        ButtonPlayAgain,
        ButtonStop,
        RoundEndScore,

        // Step 4: the profile pick, the parent gate's messages and the profiles and settings screen (screen only, not
        // spoken: the spec gives no words to speak). All wording is placeholder for the Croatian review.
        ButtonParentArea,
        ButtonBack,
        ButtonChangeProfile,
        ProfilePickTitle,
        GateWrong,
        GateWait,
        ProfilesTitle,
        ProfileNamePlaceholder,
        ProfileNameInvalid,
        ButtonSave,
        ButtonRename,
        ButtonDelete,
        ConfirmDeleteProfile,
        ButtonYes,
        ButtonNo,
        VolumeLabel,
        VolumeValue,
        SaveFailed,

        // Step 6: Veće, manje, jednako. The explanations are spoken (a wrong answer always is: FS:42); the single-clip
        // words are keys of their own, as ThenMore and Is are, so VoiceScript.csv lists them. The instruction is screen
        // only: no spoken instruction until the voice method is chosen (Q5). All wording is placeholder for the Croatian review.
        ExplainLess,
        ExplainMore,
        ExplainSame,
        ExplainGroups,
        ExplainOrder3,
        ExplainOrder4,
        ExplainOrder5,
        LessThan,
        MoreThan,
        SameAs,
        Here,
        There,
        InOrder,
        InstructionOrder,

        // Step 7: Brojevna crta. The three explanations are spoken (a wrong answer always is: FS:42); the jump back and
        // forth reuses ExplainJump and ExplainJumpBack. InFrontOf and Behind are single-clip words, as Here and There are.
        // The four instructions are screen only: no spoken instruction until the voice method is chosen (Q5). All wording
        // is placeholder for the Croatian review.
        ExplainPlace,
        ExplainBefore,
        ExplainAfter,
        InFrontOf,
        Behind,
        InstructionPlace,
        InstructionBefore,
        InstructionAfter,
        InstructionJump,
    }
}
