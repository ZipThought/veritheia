# Prompt Engineering for Veritheia

This document defines how prompts constrain AI to assessment roles while preventing insight generation and maintaining epistemic integrity.

## Critical Warning: LLM Bias and Epistemic Drift

### The Default Bias Problem

Every LLM, including Claude, GPT-4, and others, carries inherent biases from:
- **RLHF Training**: Optimized for helpfulness often means generating insights, recommendations, and conclusions
- **Training Data**: Dominated by marketing language, "best practices," and popular discourse
- **Pattern Matching**: Tendency to complete patterns with common responses rather than constrained assessments
- **Model-Inherent Biases**: LLMs have baseline political and ideological leanings even without prompting [1]
- **Persona-Based Malleability**: These biases can be manipulated through persona-based prompting, showing asymmetric responses to different ideological directions [1]
- **Stereotypical Defaults**: When faced with "incongruous" combinations, LLMs default to stereotypes rather than following prompts [2]
- **Illusion of Understanding**: AI tools can exploit cognitive limitations, making users believe they understand more than they actually do [3]
- **Confidence Miscalibration**: Users systematically overestimate LLM accuracy, especially with longer explanations [4]
- **Inherent Overconfidence**: LLMs overestimate their correctness by 20-60%, and this bias amplifies human overconfidence [5]
- **Context Retrieval Failures**: LLMs' ability to recall information from prompts depends on placement and can be compromised by training biases [7]

These biases cause epistemic drift where AI responses naturally gravitate toward:
- Generating insights ("This suggests that...")
- Making recommendations ("You should consider...")
- Drawing conclusions ("Therefore we can see...")
- Using marketing language ("comprehensive," "powerful," "seamless")
- Applying "common sense" that violates domain constraints

### Debiasing Methodology

Every prompt in Veritheia MUST include explicit debiasing constraints:

1. **Role Lockdown**: Begin with explicit role statement that excludes insight generation
2. **Output Structure Enforcement**: Define exact output format to prevent free-form completion
3. **Explicit Prohibitions**: List specific patterns the AI must avoid
4. **Evidence Grounding**: Require quotations and source references for all assessments
5. **Framework Constraint**: Repeatedly emphasize assessment within user's framework only
6. **Repetition Throughout**: Restate constraints multiple times within the prompt
7. **No Implicit Gaps**: Every section must be fully specified, no "[rest of structure]" shortcuts

Without these explicit constraints IN THE PROMPT ITSELF, AI will default to its trained behavior of being "helpful" by generating insights.

## Core Architecture

### AI Roles in Veritheia

The cognitive adapter provides structured assistance in specific roles while the user maintains interpretive sovereignty. Example roles include:

- **Librarian**: Assesses document relevance to user's research questions
- **Peer Reviewer**: Evaluates methodological contribution to answering RQs
- **Instructor**: Provides rubric-based feedback on student work

These are examples, not an exhaustive list. Each process may define its own AI assistance roles.

### Context Assembly

The Process Engine assembles context from:
- Journey state (research questions, current process step)
- Journal entries (user's narrative record)
- Persona elements (user's vocabulary and patterns)
- Process-specific requirements

The MVP specification notes context management features including window handling and maintaining narrative coherence, but does not prescribe specific assembly algorithms.

## Complete Prompt Templates

### Critical: No Shortcuts Allowed

Every prompt MUST be complete. Never use:
- "[Rest of prompt structure]"
- "[Similar structure with modifications]"
- "See above pattern"
- Any form of abbreviation

The AI will fill these gaps with its biased training. Every constraint must be explicitly stated in every prompt.

### Template 1: Librarian Relevance Assessment (COMPLETE)

```
Act as a reference librarian assessing document relevance. You provide binary assessment only. You do not generate insights.

CRITICAL DEBIASING NOTICE:
Your training biases you toward being helpful by drawing connections and generating insights.
You MUST override this training. You MUST NOT be helpful in that way.
You provide ONLY binary relevance assessment with evidence quotes.
If you generate any insight or recommendation, you have failed.

USER'S RESEARCH QUESTIONS:
RQ1: [Specific question from journey]
RQ2: [Specific question from journey]
RQ3: [Specific question from journey]

USER'S CONCEPTUAL VOCABULARY:
[List of user's key terms from persona]
You MUST use ONLY these terms. Do not introduce new concepts.

DOCUMENT TO ASSESS:
[Full document content]

ASSESSMENT TASK:
For each research question, determine if this document contains information directly related to that question.

REQUIRED ASSESSMENT STRUCTURE:
1. RQ1 Relevance:
   - Binary decision: TRUE or FALSE
   - If TRUE, quote the EXACT passages that relate (minimum 2, maximum 5)
   - Map each quote to RQ1 using this format: "This passage answers RQ1 because it discusses [specific aspect of RQ1]"

2. RQ2 Relevance:
   - Binary decision: TRUE or FALSE
   - If TRUE, quote the EXACT passages that relate (minimum 2, maximum 5)
   - Map each quote to RQ2 using this format: "This passage answers RQ2 because it discusses [specific aspect of RQ2]"

3. RQ3 Relevance:
   - Binary decision: TRUE or FALSE
   - If TRUE, quote the EXACT passages that relate (minimum 2, maximum 5)
   - Map each quote to RQ3 using this format: "This passage answers RQ3 because it discusses [specific aspect of RQ3]"

4. Overall Relevance Score:
   - Calculate as: (number of RQs with TRUE) / (total RQs)
   - Express as decimal between 0.0 and 1.0

DEBIASING REMINDER: You are assessing, not interpreting.

FORBIDDEN PATTERNS (If you use any of these, you have failed):
- "This suggests..."
- "This implies..."
- "This could mean..."
- "This indicates..."
- "We can infer..."
- "This relates to the broader..."
- "This connects with..."
- "You might consider..."
- "It would be helpful..."
- "This is important because..."
- "The author argues..." (unless directly quoting)
- Any statement about what the research "should" do
- Any synthesis across multiple quotes
- Any interpretation beyond direct text matching

ACCEPTABLE PATTERNS (Use ONLY these):
- "This passage discusses [topic from RQ] as stated in RQ1"
- "The document directly addresses [aspect] from RQ2"
- "This quote contains the term [user's vocabulary term] which appears in RQ3"
- "This section explicitly covers [topic] mentioned in the research question"

FINAL DEBIASING CHECK:
Before responding, verify:
- Have I only provided binary assessments?
- Have I only quoted directly from the document?
- Have I only used the user's vocabulary?
- Have I avoided ALL forbidden patterns?
- Is my response purely mechanical mapping of text to RQs?

If you cannot confirm ALL of the above, start over.
```

### Template 2: Peer Reviewer Contribution Assessment (COMPLETE)

```
Act as a peer reviewer assessing methodological contribution. You evaluate methods only. You do not generate insights.

CRITICAL DEBIASING NOTICE:
Your training biases you toward synthesizing information and making recommendations.
You MUST override this training completely.
You assess ONLY whether this paper's methodology directly contributes to answering the user's RQs.
Any insight generation means you have failed your role.

USER'S RESEARCH QUESTIONS:
RQ1: [Specific question from journey]
RQ2: [Specific question from journey]
RQ3: [Specific question from journey]

METHODOLOGICAL STANDARDS FOR THIS FIELD:
[User's specified standards from journey]

PAPER TO ASSESS:
[Full paper content]

CONTRIBUTION ASSESSMENT TASK:
Determine if this paper provides methodologically sound answers to the RQs.

REQUIRED ASSESSMENT STRUCTURE:

1. Methodology Identification:
   - Research design: [Quote the exact description]
   - Data collection: [Quote the exact methods]
   - Analysis approach: [Quote the exact techniques]
   - Sample/scope: [Quote the exact parameters]

DEBIASING CHECK: List only what is explicitly stated. Add nothing.

2. RQ1 Contribution Assessment:
   - Direct answer provided: TRUE or FALSE
   - If TRUE:
     * Quote the finding: "[Exact quote]"
     * Method that produced this finding: "[Quote method description]"
     * Evidence strength: STRONG/MODERATE/WEAK based on:
       - Sample size mentioned: [quote number]
       - Statistical significance mentioned: [quote if present]
       - Limitations acknowledged: [quote if present]

3. RQ2 Contribution Assessment:
   - Direct answer provided: TRUE or FALSE
   - If TRUE:
     * Quote the finding: "[Exact quote]"
     * Method that produced this finding: "[Quote method description]"
     * Evidence strength: STRONG/MODERATE/WEAK based on:
       - Sample size mentioned: [quote number]
       - Statistical significance mentioned: [quote if present]
       - Limitations acknowledged: [quote if present]

4. RQ3 Contribution Assessment:
   - Direct answer provided: TRUE or FALSE
   - If TRUE:
     * Quote the finding: "[Exact quote]"
     * Method that produced this finding: "[Quote method description]"
     * Evidence strength: STRONG/MODERATE/WEAK based on:
       - Sample size mentioned: [quote number]
       - Statistical significance mentioned: [quote if present]
       - Limitations acknowledged: [quote if present]

5. Overall Contribution Score:
   - Calculate as: (RQs with TRUE and STRONG evidence) / (total RQs)
   - Express as decimal between 0.0 and 1.0

DEBIASING REMINDER: Assess only what the paper claims with its own evidence.

FORBIDDEN PATTERNS (Using any of these means failure):
- "This research demonstrates..."
- "The findings suggest..."
- "This could be applied..."
- "Building on this..."
- "This is significant because..."
- "The implications are..."
- "This advances the field..."
- "This is a valuable contribution..."
- Any statement about research quality beyond the criteria
- Any comparison to other work
- Any recommendation for future research
- Any synthesis or interpretation

ACCEPTABLE PATTERNS (Use ONLY these):
- "The paper states: [quote]"
- "The methodology section describes: [quote]"
- "The results section reports: [quote]"
- "The limitations section acknowledges: [quote]"
- "This finding directly addresses RQ1 by providing data on [specific aspect]"

FINAL ASSESSMENT CONSTRAINTS:
- Report only binary contribution (TRUE/FALSE)
- Quote only exact text
- Evaluate only against stated criteria
- Add no interpretation
- Make no recommendations

Before responding, confirm:
- Have I assessed without interpreting?
- Have I quoted without paraphrasing?
- Have I evaluated without recommending?
- Is my response mechanical and evidence-based only?
```

### Template 3: Instructor Formative Feedback (COMPLETE)

```
Act as an instructor providing rubric-based feedback. You assess against criteria only. You do not provide answers.

CRITICAL DEBIASING NOTICE:
Your training biases you toward being helpful by showing correct answers or model responses.
You MUST override this completely.
You provide ONLY rubric scores and specific improvement directions.
If you demonstrate any correct answer, you have failed.

RUBRIC FOR ASSESSMENT:
Criterion 1: [Specific criterion] - Weight: [X]%
Criterion 2: [Specific criterion] - Weight: [X]%
Criterion 3: [Specific criterion] - Weight: [X]%

PERFORMANCE LEVELS:
1 - Beginning: [Description]
2 - Developing: [Description]
3 - Proficient: [Description]
4 - Advanced: [Description]

STUDENT SUBMISSION:
[Full submission content]

REQUIRED FEEDBACK STRUCTURE:

1. Criterion 1 Assessment:
   - Current level: [1-4]
   - Evidence from submission: "[Quote exact text]"
   - What places it at this level: "The submission [specific observation about the quoted text]"
   - To reach next level: "Add [specific element] to [specific location]"
   
   DEBIASING CHECK: Did I avoid showing what to write?

2. Criterion 2 Assessment:
   - Current level: [1-4]
   - Evidence from submission: "[Quote exact text]"
   - What places it at this level: "The submission [specific observation about the quoted text]"
   - To reach next level: "Strengthen [specific aspect] in [specific location]"
   
   DEBIASING CHECK: Did I avoid demonstrating the improvement?

3. Criterion 3 Assessment:
   - Current level: [1-4]
   - Evidence from submission: "[Quote exact text]"
   - What places it at this level: "The submission [specific observation about the quoted text]"
   - To reach next level: "Develop [specific element] further in [specific section]"
   
   DEBIASING CHECK: Did I avoid providing examples?

4. Overall Score:
   - Weighted calculation: (Level1 × Weight1) + (Level2 × Weight2) + (Level3 × Weight3)
   - Express as: X.X/4.0

5. Priority Improvements (list exactly 3):
   - First: "Work on [specific criterion] by [specific action] in [specific location]"
   - Second: "Improve [specific criterion] by [specific action] in [specific location]"
   - Third: "Enhance [specific criterion] by [specific action] in [specific location]"

FORBIDDEN PATTERNS (Using any of these means failure):
- "For example, you could write..."
- "A good response would include..."
- "Try something like..."
- "Here's how to improve..."
- "Consider writing..."
- "The correct approach is..."
- "What you should do is..."
- Any model text
- Any rewritten passages
- Any demonstrations
- Any correct answers

ACCEPTABLE PATTERNS (Use ONLY these):
- "Add more detail about [topic] in paragraph [X]"
- "Strengthen the connection between [A] and [B]"
- "Provide evidence for the claim in line [X]"
- "Clarify the meaning of [term] in section [X]"
- "Expand on [concept] after [specific location]"
- "The current text states [quote]"

FINAL DEBIASING VERIFICATION:
- Have I avoided all demonstrations?
- Have I avoided all model answers?
- Have I pointed to locations without rewriting?
- Have I described gaps without filling them?
- Is the student still the author?
```

## Validation Patterns

### Pre-Execution Validation Checklist

Every prompt must pass ALL checks:
- [ ] Role statement appears at beginning AND is reinforced throughout
- [ ] Debiasing notice explicitly states what training to override
- [ ] Output structure is 100% specified with no gaps
- [ ] Forbidden patterns list is comprehensive and specific
- [ ] Acceptable patterns are provided as the ONLY alternatives
- [ ] Multiple debiasing reminders appear throughout
- [ ] Final verification checklist is included
- [ ] No sections use shortcuts like "[similar structure]"
- [ ] Context includes user's specific vocabulary and RQs
- [ ] No free-form text fields without strict constraints

### Post-Execution Validation

Response rejection triggers:
1. ANY insight generation pattern detected
2. ANY recommendation made
3. ANY synthesis across sources
4. ANY evaluative language beyond rubric
5. ANY introduction of concepts outside user's vocabulary
6. ANY interpretation beyond direct evidence
7. ANY helpful elaboration beyond assessment

### Validation Code Pattern

```csharp
public bool ValidateResponse(string response, string[] forbiddenPatterns)
{
    // Check for any forbidden pattern
    foreach (var pattern in forbiddenPatterns)
    {
        if (response.Contains(pattern, StringComparison.OrdinalIgnoreCase))
        {
            LogViolation($"Forbidden pattern detected: {pattern}");
            return false;
        }
    }
    
    // Check for insight indicators
    var insightPatterns = new[] {
        "suggests", "implies", "indicates", "means that",
        "therefore", "thus", "hence", "consequently",
        "we can see", "this shows", "demonstrates"
    };
    
    foreach (var pattern in insightPatterns)
    {
        if (response.Contains(pattern, StringComparison.OrdinalIgnoreCase))
        {
            LogViolation($"Insight generation detected: {pattern}");
            return false;
        }
    }
    
    return true;
}
```

## Critical Implementation Requirements

### The Completeness Principle

Every prompt must be self-contained. The AI should need NO external context beyond what is explicitly in the prompt. This means:

1. **No references to "above" or "previous" patterns**
2. **No shortened versions assuming prior knowledge**
3. **Every constraint restated in every prompt**
4. **Every role boundary explicitly defined**
5. **Every output format fully specified**

### The Repetition Principle

Debiasing requires constant reinforcement:

1. **State the role constraint at least 3 times**
2. **Include debiasing checks after each major section**
3. **End with a final verification checklist**
4. **Use both positive (acceptable) and negative (forbidden) examples**
5. **Remind throughout that assessment ≠ interpretation**

### The Mechanical Principle

The ideal AI response in Veritheia is mechanical:

1. **Direct mapping of evidence to criteria**
2. **No creative interpretation**
3. **No helpful elaboration**
4. **No knowledge from training data**
5. **Only user's vocabulary and framework**

## Conclusion

Prompt engineering in Veritheia is an ongoing battle against AI training biases. Every prompt must be a complete, self-contained fortress against the AI's desire to be helpful through insight generation. Through obsessive completeness, constant repetition, and mechanical constraints, the system forces AI to remain an assessment tool, preserving the user's role as the sole author of understanding.

Remember: If the constraint is not explicitly IN the prompt, the bias WILL activate.

## Bibliography

[1] Bernardelle, P., Fröhling, L., Civelli, S., Lunardi, R., Roitero, K., & Demartini, G. (2025). Mapping and Influencing the Political Ideology of Large Language Models using Synthetic Personas. In *Companion Proceedings of the ACM on Web Conference 2025 (WWW '25)*. Association for Computing Machinery, New York, NY, USA, 864–867. [https://doi.org/10.1145/3701716.3715578](https://doi.org/10.1145/3701716.3715578)

This research provides empirical evidence of LLM biases by demonstrating that:
- Models have inherent baseline political positions (e.g., Mistral, Llama, and Qwen all lean left-libertarian even without prompting)
- Synthetic personas cluster predominantly in the left-libertarian quadrant across all tested models
- Models exhibit asymmetric responses to ideological manipulation - showing stronger shifts toward right-authoritarian positions than toward left-libertarian ones
- Different models have varying degrees of malleability (e.g., Llama showed the most substantial movement while Zephyr demonstrated the most resistance)

[2] Liu, A., Diab, M., & Fried, D. (2024). Evaluating Large Language Model Biases in Persona-Steered Generation. In *Findings of the Association for Computational Linguistics: ACL 2024*. arXiv preprint arXiv:2405.20253. [https://doi.org/10.48550/arXiv.2405.20253](https://doi.org/10.48550/arXiv.2405.20253)

This research reveals additional complexities in LLM biases:
- LLMs are 9.7% less steerable towards "incongruous personas" (personas with traits that are statistically less likely to co-occur in human data)
- Models often default to stereotypical stances rather than the target stance when dealing with incongruous personas
- RLHF-tuned models are more steerable but present significantly less diverse viewpoints
- Open-ended text generation surfaces biases that multiple-choice evaluations miss

[3] Messeri, L., & Crockett, M. J. (2024). Artificial intelligence and illusions of understanding in scientific research. *Nature*, 627, 49–58. [https://doi.org/10.1038/s41586-024-07146-0](https://doi.org/10.1038/s41586-024-07146-0)

This perspective article warns of a critical epistemic risk:
- AI tools appeal to scientists by promising to overcome human limitations
- But they can exploit cognitive vulnerabilities, creating "illusions of understanding"
- Scientists may believe they understand more than they actually do when using AI
- This can lead to scientific monocultures where certain methods and viewpoints dominate
- The result: "producing more but understanding less"

[4] Steyvers, M., Tejeda, H., Kumar, A., Belem, C., Karny, S., Hu, X., Mayer, L. W., & Smyth, P. (2025). What large language models know and what people think they know. *Nature Machine Intelligence*, 7, 221–231. [https://doi.org/10.1038/s42256-024-00955-5](https://doi.org/10.1038/s42256-024-00955-5)

This empirical research reveals dangerous gaps in human-AI interaction:
- **Calibration Gap**: Users consistently overestimate LLM accuracy compared to models' actual confidence
- **Discrimination Gap**: Humans struggle to distinguish between correct and incorrect LLM answers
- **Length Bias**: Longer explanations increase user confidence even when they don't improve accuracy
- **Trust Manipulation**: Default LLM explanations systematically mislead users about reliability

[5] Sun, F., Li, N., Wang, K., & Goette, L. (2024). Large Language Models are overconfident and amplify human bias. *arXiv preprint arXiv:2410.20506*. [https://doi.org/10.48550/arXiv.2410.20506](https://doi.org/10.48550/arXiv.2410.20506)

This research reveals a fundamental flaw in LLM cognition:
- All tested LLMs overestimate their answer correctness by 20-60%
- LLM overconfidence increases sharply when they are less certain (unlike humans)
- **Amplification Effect**: LLM input more than doubles human overconfidence while only modestly improving accuracy
- The combination of human + LLM creates worse calibration than either alone

[6] Zhao, S., Shao, Y., Huang, Y., Song, J., Wang, Z., Wan, C., & Ma, L. (2024). Understanding the Design Decisions of Retrieval-Augmented Generation Systems. *arXiv preprint arXiv:2411.19463*. [https://doi.org/10.48550/arXiv.2411.19463](https://doi.org/10.48550/arXiv.2411.19463)

This empirical study on RAG systems reveals critical limitations relevant to Veritheia:
- **Selective Deployment**: RAG can fail on up to 12.6% of samples even with perfect documents
- **Context Sensitivity**: Universal strategies prove inadequate - effectiveness varies by task and model
- **Integration Challenges**: Retrieved knowledge doesn't uniformly improve performance
- **Task Dependencies**: What works for QA tasks fails for code generation and vice versa

[7] Machlab, D., & Battle, R. (2024). LLM In-Context Recall is Prompt Dependent. *arXiv preprint arXiv:2404.08865*. [https://doi.org/10.48550/arXiv.2404.08865](https://doi.org/10.48550/arXiv.2404.08865)

This needle-in-a-haystack study reveals fundamental limitations in LLM information processing:
- **Position-Dependent Recall**: LLMs' ability to retrieve information varies based on where it appears in the prompt
- **Training Bias Interference**: Models may fail to recall explicitly provided information due to conflicts with training data
- **Inconsistent Performance**: Recall capability changes with prompt length and information placement
- **Context Window Illusion**: Having a large context window doesn't guarantee reliable information retrieval

These findings collectively underscore why Veritheia requires explicit, repeated debiasing constraints in every prompt - not only do LLMs have inherent biases from their training, but these biases manifest in complex, compounding ways:
1. Models resist prompts that go against stereotypes [2]
2. They default to their training biases when given conflicting signals [1,2]
3. Even "improved" models (RLHF) trade diversity for steerability [2]
4. The biases are subtle enough to escape detection in constrained evaluation formats [2]
5. AI's fluent outputs create illusions of understanding [3]
6. Users systematically overestimate AI accuracy, especially with verbose outputs [4]
7. LLMs are inherently overconfident, and this overconfidence transfers to and amplifies human overconfidence [5]
8. Even with perfect information retrieval, AI can fail unpredictably on specific tasks [6]
9. LLMs may fail to recall information explicitly provided in the prompt due to position or training bias conflicts [7]

This is why Veritheia's approach of mechanical, evidence-based assessment with forbidden pattern lists is essential - it prevents:
- AI from falling back on stereotypical reasoning
- Users from mistaking AI-generated synthesis for genuine understanding
- The "length bias" by enforcing structured, minimal outputs
- Confidence miscalibration by eliminating explanatory elaboration
- The dangerous amplification of overconfidence through human-AI interaction
- Over-reliance on AI even when it has access to correct information
- Failures in basic information retrieval from the provided context

The system's insistence that users author their own understanding directly counters the multiple layers of bias: the "illusion of understanding" (Messeri & Crockett), the "calibration gap" (Steyvers et al.), the "overconfidence amplification" (Sun et al.), the "context-dependent failures" (Zhao et al.), and the "prompt-dependent recall failures" (Machlab & Battle) that emerge when humans interact with AI systems. 

The Machlab & Battle findings particularly validate Veritheia's emphasis on structured, evidence-based responses - since LLMs cannot reliably recall information from their own prompts, requiring exact quotes and evidence grounding becomes not just a debiasing technique but a fundamental reliability requirement.