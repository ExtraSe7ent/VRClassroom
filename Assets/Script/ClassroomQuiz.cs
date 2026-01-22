using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class ClassroomQuiz : MonoBehaviourPunCallbacks
{
    [Header("UI REFERENCES")]
    public GameObject teacherPanel;
    public GameObject studentPanel;
    
    public TMP_Text questionText;         
    public TMP_Text[] answerTexts;        
    public TMP_Text scoreText;            
    public TMP_Text statusText;           
    
    [Header("SETTINGS")]
    private string folderPath; 
    
    private string questionFileName = "Questions.csv";
    private string resultFileName = "Score.csv"; 

    private List<QuestionData> questionList = new List<QuestionData>();
    private int currentQIndex = 0;
    private int correctCount = 0;
    private Dictionary<string, string> studentScores = new Dictionary<string, string>();

    void Start()
    {
        string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        if (!basePath.EndsWith("Documents"))
        {
            basePath = Path.Combine(basePath, "Documents");
        }

        folderPath = Path.Combine(basePath, "Unity", "File_cau_hoi");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        teacherPanel.SetActive(false);
        studentPanel.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            teacherPanel.SetActive(true);
            if (statusText != null)
                statusText.text = $"Ready.";
        }
        else
        {
            if (statusText != null)
                statusText.text = "Waiting for teacher...";
        }
    }

    public void OnTeacherStartBtn()
    {
        string fullPath = Path.Combine(folderPath, questionFileName);
        
        if (!File.Exists(fullPath))
        {
            if (statusText != null) statusText.text = $"Error: File missing at:\n{fullPath}";
            return;
        }

        string csvContent = File.ReadAllText(fullPath);
        photonView.RPC("RPC_ReceiveQuestions", RpcTarget.All, csvContent);
    }

    public void OnTeacherExportBtn()
    {
        string fullPath = Path.Combine(folderPath, resultFileName);
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("Student Name,Score,Date");
        foreach (var item in studentScores) 
        {
            sb.AppendLine($"{item.Key},{item.Value},{System.DateTime.Now}");
        }

        File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
        if (statusText != null) statusText.text = "Exported to:\n" + fullPath;
    }

    [PunRPC]
    void RPC_ReceiveQuestions(string csvContent)
    {
        bool success = ParseCSV(csvContent);
        
        if (!success)
        {
             if (PhotonNetwork.IsMasterClient && statusText != null) 
                statusText.text = "Error parsing CSV file. Check Console.";
             return;
        }

        currentQIndex = 0;
        correctCount = 0;
        
        if (PhotonNetwork.IsMasterClient)
        {
            if (statusText != null) statusText.text = "Quiz Started!";
            teacherPanel.SetActive(true);
            studentPanel.SetActive(false);
        }
        else
        {
            teacherPanel.SetActive(false);
            studentPanel.SetActive(true);
            
            if (questionText != null) questionText.gameObject.SetActive(true);
            foreach(var txt in answerTexts)
            {
                if(txt != null && txt.transform.parent != null)
                    txt.transform.parent.gameObject.SetActive(true);
            }
            if (scoreText != null) scoreText.gameObject.SetActive(false);

            DisplayQuestion();
        }
    }

    [PunRPC]
    void RPC_SendScoreToTeacher(string studentName, string score)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (studentScores.ContainsKey(studentName)) 
            studentScores[studentName] = score;
        else 
            studentScores.Add(studentName, score);

        if (statusText != null) 
            statusText.text = $"Student '{studentName}' submitted ({score}).\nTotal: {studentScores.Count}";
    }

    void DisplayQuestion()
    {
        if (currentQIndex < questionList.Count)
        {
            QuestionData q = questionList[currentQIndex];
            
            if (questionText != null) 
                questionText.text = $"Q{currentQIndex + 1}: {q.question}";
            
            if (answerTexts.Length >= 4)
            {
                answerTexts[0].text = "A. " + q.ans1;
                answerTexts[1].text = "B. " + q.ans2;
                answerTexts[2].text = "C. " + q.ans3;
                answerTexts[3].text = "D. " + q.ans4;
            }
        }
        else
        {
            FinishQuiz();
        }
    }

    public void OnAnswerSelected(int index)
    {
        if (currentQIndex >= questionList.Count) return;

        if (questionList[currentQIndex].correctAnswer == index) 
        {
            correctCount++;
        }
        
        currentQIndex++;
        DisplayQuestion();
    }

    void FinishQuiz()
    {

        string finalScore = $"{correctCount}/{questionList.Count}";
        
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = $"CONGRATULATIONS!\nYour Score: {finalScore}";
        }

        if (questionText != null) 
            questionText.gameObject.SetActive(false);

        if (answerTexts != null)
        {
            foreach(var txt in answerTexts)
            {
                if(txt != null && txt.transform.parent != null)
                {
                    txt.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        photonView.RPC("RPC_SendScoreToTeacher", RpcTarget.MasterClient, PhotonNetwork.NickName, finalScore);
    }

    bool ParseCSV(string content)
    {
        questionList.Clear();
        
        string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split(';');

            if (parts.Length < 6)
            {
                parts = line.Split(',');
            }
            
            if (parts.Length < 6)
            {
                parts = line.Split('\t');
            }
            
            if (parts.Length >= 6) 
            {
                try 
                {
                    QuestionData q = new QuestionData();
                    q.question = parts[0].Trim();
                    q.ans1 = parts[1].Trim(); 
                    q.ans2 = parts[2].Trim(); 
                    q.ans3 = parts[3].Trim(); 
                    q.ans4 = parts[4].Trim();
                    
                    int rawAnswer;
                    if (int.TryParse(parts[5].Trim(), out rawAnswer))
                    {
                        q.correctAnswer = rawAnswer - 1; 
                    }
                    else 
                    {
                        Debug.LogError($"Parse Error at line {i}: Answer '{parts[5]}' is not a number.");
                        continue;
                    }
                    
                    questionList.Add(q);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading line {i}: {e.Message}");
                }
            }
        }

        if (questionList.Count == 0) return false;
        
        return true;
    }

    class QuestionData 
    { 
        public string question;
        public string ans1, ans2, ans3, ans4; 
        public int correctAnswer; 
    }
}