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
    }
}
