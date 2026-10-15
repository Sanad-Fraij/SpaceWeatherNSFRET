using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
#if TMPro
using TMPro;
#endif

public class MultiPageQuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        [TextArea(2, 5)]
        public string questionText = "Enter question here...";

        [Tooltip("Add as many multiple-choice options as you want for this question.")]
        public List<string> choiceAnswers = new List<string> { "Option A", "Option B", "Option C", "Option D" };

        [Tooltip("Zero-based index of the correct answer (0 = 1st option, 1 = 2nd option, etc.).")]
        public int correctAnswerIndex = 0;
    }

    [Header("UI Prefabs")]
    public GameObject questionTextPrefab;
    public GameObject answerButtonPrefab;
    public GameObject leftArrowPrefab;
    public GameObject rightArrowPrefab;
    public GameObject submitButtonPrefab;

    [Header("Layout Container")]
    public Transform contentContainer;

    [Header("Layout Offsets & Spacing")]
    [Tooltip("Offset (X, Y) relative to container for Question Text.")]
    public Vector2 questionPositionOffset = new Vector2(0f, 150f);

    [Tooltip("Offset (X, Y) relative to container for the starting position of Answers.")]
    public Vector2 answersPositionOffset = new Vector2(0f, 50f);

    [Tooltip("Adjustable vertical spacing between multiple choice buttons to prevent overlapping.")]
    public float answerVerticalSpacing = 60f;

    [Tooltip("Position offset (X,Y) for the Left Arrow button.")]
    public Vector2 leftArrowOffset = new Vector2(-250f, -200f);

    [Tooltip("Position offset (X,Y) for the Right Arrow button.")]
    public Vector2 rightArrowOffset = new Vector2(250f, -200f);

    [Tooltip("Position offset (X,Y) for the Submit button on the final page.")]
    public Vector2 submitButtonOffset = new Vector2(0f, -50f);

    [Header("Quiz Questions")]
    public List<QuestionData> questions = new List<QuestionData>();

    private int currentPageIndex = 0;
    private int[] userSelectedAnswers;

    private List<GameObject> activePageElements = new List<GameObject>();
    private List<Toggle> currentOptionToggles = new List<Toggle>();

    void OnValidate()
    {
        if (questions != null)
        {
            foreach (var q in questions)
            {
                if (q.choiceAnswers != null && q.choiceAnswers.Count > 0)
                {
                    q.correctAnswerIndex = Mathf.Clamp(q.correctAnswerIndex, 0, q.choiceAnswers.Count - 1);
                }
                else
                {
                    q.correctAnswerIndex = 0;
                }
            }
        }
    }

    void Start()
    {
        if (contentContainer == null)
        {
            contentContainer = transform;
        }

        if (questions == null || questions.Count == 0)
        {
            Debug.LogWarning("[QuizManager] No questions assigned!");
            return;
        }

        userSelectedAnswers = new int[questions.Count];
        for (int i = 0; i < userSelectedAnswers.Length; i++)
        {
            userSelectedAnswers[i] = -1;
        }

        BuildCurrentPage();
    }

    private void BuildCurrentPage()
    {
        ClearCurrentPage();

        // 1. SUBMIT PAGE (Appears on page after last question)
        if (currentPageIndex == questions.Count)
        {
            BuildSubmitPage();
            return;
        }

        if (currentPageIndex < 0 || currentPageIndex >= questions.Count) return;

        QuestionData currentQuestion = questions[currentPageIndex];

        // 2. Instantiate Question Text Prefab & Populate Text from QuestionData
        if (questionTextPrefab != null)
        {
            GameObject qObj = Instantiate(questionTextPrefab, contentContainer);
            activePageElements.Add(qObj);

            RectTransform rect = qObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = questionPositionOffset;
            }

            // Set question text prompt from the Inspector QuestionData entry
            string formattedPrompt = $"Q{currentPageIndex + 1}: {currentQuestion.questionText}";
            SetTextOnObject(qObj, formattedPrompt);
        }

        // 3. Dynamic Answer Button Prefabs
        currentOptionToggles.Clear();
        int choiceCount = currentQuestion.choiceAnswers != null ? currentQuestion.choiceAnswers.Count : 0;

        for (int i = 0; i < choiceCount; i++)
        {
            int choiceIndex = i;

            if (answerButtonPrefab != null)
            {
                GameObject btnObj = Instantiate(answerButtonPrefab, contentContainer);
                activePageElements.Add(btnObj);

                // Apply vertical spacing offset for each choice button
                RectTransform rect = btnObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Vector2 calculatedPos = answersPositionOffset - new Vector2(0f, choiceIndex * answerVerticalSpacing);
                    rect.anchoredPosition = calculatedPos;
                }

                // Set option text on button prefab
                SetTextOnObject(btnObj, currentQuestion.choiceAnswers[choiceIndex]);

                Toggle toggleComp = btnObj.GetComponentInChildren<Toggle>();
                if (toggleComp != null)
                {
                    currentOptionToggles.Add(toggleComp);

                    bool isSelected = (userSelectedAnswers[currentPageIndex] == choiceIndex);
                    toggleComp.SetIsOnWithoutNotify(isSelected);

                    toggleComp.onValueChanged.AddListener((isOn) =>
                    {
                        if (isOn) SelectOption(choiceIndex);
                    });
                }
                else
                {
                    Button btnComp = btnObj.GetComponentInChildren<Button>();
                    if (btnComp != null)
                    {
                        btnComp.onClick.AddListener(() => SelectOption(choiceIndex));
                    }
                }
            }
        }

        // 4. Arrow Navigation Buttons
        if (currentPageIndex > 0 && leftArrowPrefab != null)
        {
            GameObject leftArrow = Instantiate(leftArrowPrefab, contentContainer);
            activePageElements.Add(leftArrow);

            RectTransform rect = leftArrow.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = leftArrowOffset;

            Button leftBtn = leftArrow.GetComponentInChildren<Button>();
            if (leftBtn != null) leftBtn.onClick.AddListener(GoToPreviousPage);
        }

        if (rightArrowPrefab != null)
        {
            GameObject rightArrow = Instantiate(rightArrowPrefab, contentContainer);
            activePageElements.Add(rightArrow);

            RectTransform rect = rightArrow.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = rightArrowOffset;

            Button rightBtn = rightArrow.GetComponentInChildren<Button>();
            if (rightBtn != null) rightBtn.onClick.AddListener(GoToNextPage);
        }
    }

    private void BuildSubmitPage()
    {
        // Display Submission Prompt
        if (questionTextPrefab != null)
        {
            GameObject qObj = Instantiate(questionTextPrefab, contentContainer);
            activePageElements.Add(qObj);

            RectTransform rect = qObj.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = questionPositionOffset;

            SetTextOnObject(qObj, "You have reached the end of the quiz! Click Submit to check your answers.");
        }

        // Left Arrow to allow navigating back to revise answers
        if (leftArrowPrefab != null)
        {
            GameObject leftArrow = Instantiate(leftArrowPrefab, contentContainer);
            activePageElements.Add(leftArrow);

            RectTransform rect = leftArrow.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = leftArrowOffset;

            Button leftBtn = leftArrow.GetComponentInChildren<Button>();
            if (leftBtn != null) leftBtn.onClick.AddListener(GoToPreviousPage);
        }

        // Submit Button
        if (submitButtonPrefab != null)
        {
            GameObject submitObj = Instantiate(submitButtonPrefab, contentContainer);
            activePageElements.Add(submitObj);

            RectTransform rect = submitObj.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = submitButtonOffset;

            Button submitBtn = submitObj.GetComponentInChildren<Button>();
            if (submitBtn != null) submitBtn.onClick.AddListener(VerifyAndSubmitQuiz);
        }
    }

    /// <summary>
    /// Helper to assign text to either standard UI Text or TextMeshProUGUI on target target object.
    /// </summary>
    private void SetTextOnObject(GameObject targetObj, string textValue)
    {
        if (targetObj == null) return;

        // Try standard UI Text
        Text uiText = targetObj.GetComponentInChildren<Text>();
        if (uiText != null)
        {
            uiText.text = textValue;
            return;
        }

#if TMPro
        // Try TextMeshProUGUI
        TMP_Text tmpText = targetObj.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = textValue;
        }
#endif
    }

    private void SelectOption(int selectedChoiceIndex)
    {
        userSelectedAnswers[currentPageIndex] = selectedChoiceIndex;

        for (int i = 0; i < currentOptionToggles.Count; i++)
        {
            if (currentOptionToggles[i] != null)
            {
                currentOptionToggles[i].SetIsOnWithoutNotify(i == selectedChoiceIndex);
            }
        }

        Debug.Log($"[QuizManager] Question {currentPageIndex + 1}: Selected Option {selectedChoiceIndex + 1}");
    }

    public void GoToNextPage()
    {
        if (currentPageIndex <= questions.Count - 1)
        {
            currentPageIndex++;
            BuildCurrentPage();
        }
    }

    public void GoToPreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            BuildCurrentPage();
        }
    }

    public void VerifyAndSubmitQuiz()
    {
        Debug.Log("--- QUIZ RESULTS SUBMISSION ---");
        int correctCount = 0;

        for (int i = 0; i < questions.Count; i++)
        {
            int selected = userSelectedAnswers[i];
            int correctKey = questions[i].correctAnswerIndex;

            if (selected == -1)
            {
                Debug.Log($"Question {i + 1}: WRONG (Unanswered)");
            }
            else if (selected == correctKey)
            {
                Debug.Log($"Question {i + 1}: CORRECT");
                correctCount++;
            }
            else
            {
                Debug.Log($"Question {i + 1}: WRONG (Selected Option {selected + 1}, Correct was Option {correctKey + 1})");
            }
        }

        Debug.Log($"FINAL SCORE: {correctCount} / {questions.Count} Correct!");
    }

    private void ClearCurrentPage()
    {
        foreach (GameObject obj in activePageElements)
        {
            if (obj != null) Destroy(obj);
        }
        activePageElements.Clear();
        currentOptionToggles.Clear();
    }
}