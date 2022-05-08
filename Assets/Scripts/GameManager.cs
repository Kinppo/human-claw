using System.Collections;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum GameState
{
    Start,
    Instantiate,
    Rotate,
    Collect,
    Play,
    SpeedRush,
    Lose,
    Win
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; protected set; }
    public static GameState gameState = GameState.Start;
    public int level;
    public int speedRushMax;
    public float speedRushDuration;
    public Camera cam;
    public Transform instantiatePoint;
    public Transform camPoint;
    public ParticleSystem confetti;
    public ParticleSystem leftConfetti;
    public ParticleSystem rightConfetti;
    public ParticleSystem speedRushEffect;
    public Transform wire;
    public Transform finishLine;
    public Material building;
    public Material fog;
    public Material skybox;
    public Material obstacle1;
    public Material obstacle2;
    public Texture greenTexture;
    public Texture yellowTexture;
    public Texture redTexture;
    public Material wire1;
    public Material wire2;
    public List<GameObject> cams = new List<GameObject>();
    public List<GameObject> toys = new List<GameObject>();
    public List<Color32> colors = new List<Color32>();
    public List<Color32> wireColors = new List<Color32>();
    public List<Level> levels = new List<Level>();
    [HideInInspector] public Level selectedLevel;
    [HideInInspector] public int speedCounter;
    private int toysCounter;

    void Awake()
    {
        Instance = this;
        level = PlayerPrefs.GetInt("level", 1);
        gameState = GameState.Instantiate;
        LoadLevel();
        TinySauce.OnGameStarted(level.ToString());
    }

    public void Win()
    {
        gameState = GameState.Win;
        confetti.Play();
        TinySauce.OnGameFinished(true, 1, level.ToString());
        Invoke(nameof(EnableWinPanel), 1.5f);
    }

    public void EnableWinPanel()
    {
        UIManager.Instance.SetPanel(GameState.Win);
    }

    public void Lose()
    {
        UIManager.Instance.SetPanel(GameState.Lose);
        TinySauce.OnGameFinished(false, 1, level.ToString());
    }

    [SkipRename]
    public void Restart()
    {
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySound(AudioManager.Instance.click);
        SceneManager.LoadScene(0);
    }

    [SkipRename]
    public void Next()
    {
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySound(AudioManager.Instance.click);
        level++;
        PlayerPrefs.SetInt("level", level);
        SceneManager.LoadScene(0);
        LoadLevel();
    }

    private void LoadLevel()
    {
        var l = level;
        if (l > levels.Count)
            l = Random.Range(2, levels.Count + 1);

        selectedLevel = levels[l - 1];
        selectedLevel.gameObject.SetActive(true);
        var point = selectedLevel.path.bezierPath.GetPoint(selectedLevel.path.bezierPath.NumPoints - 1);
        finishLine.position = point + new Vector3(0, -0.5f, -2);

        building.SetColor("_BaseColor", colors[SelectColor(l)]);
        fog.SetColor("_Color", colors[SelectColor(l)]);
        skybox.SetColor("_BgColor1", colors[SelectColor(l)]);
        skybox.SetColor("_BgColor2", colors[SelectColor(l)]);
        skybox.SetColor("_BgColor3", colors[SelectColor(l)]);
        RenderSettings.fogColor = colors[SelectColor(l)];
        obstacle1.SetTexture("_BaseMap", yellowTexture);
        obstacle2.SetTexture("_BaseMap", redTexture);
        wire1.SetColor("_BaseColor", wireColors[0]);
        wire2.SetColor("_BaseColor", wireColors[1]);

        var nbr = Random.Range(0, toys.Count - 2);
        StartCoroutine(InstantiateToy(nbr));
    }

    private static int SelectColor(int l)
    {
        var index = 0;
        if (l is > 2 and < 5)
            index = 1;
        else if (l is > 4 and < 7)
            index = 2;
        else if (l is > 6 and < 8)
            index = 3;
        else if (l >= 8) index = 4;

        return index;
    }

    private void ChangeEnvironment(Color32 color, float time)
    {
        StartCoroutine(LerpMaterials(time, color));
    }

    private bool isCycling;

    IEnumerator LerpMaterials(float cycleTime, Color32 color)
    {
        isCycling = true;
        float currentTime = 0;
        while (currentTime < cycleTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / cycleTime;
            Color currentColor = Color.Lerp(building.color, color, t);
            building.SetColor("_BaseColor", currentColor);
            fog.SetColor("_Color", currentColor);
            skybox.SetColor("_BgColor1", currentColor);
            skybox.SetColor("_BgColor2", currentColor);
            skybox.SetColor("_BgColor3", currentColor);
            RenderSettings.fogColor = currentColor;
            yield return null;
        }

        isCycling = false;
    }

    private IEnumerator InstantiateToy(int index)
    {
        var pos1 = new Vector3(Random.Range(-0.75f, 0.75f), 0, Random.Range(-1.1f, 1.1f));
        var pos2 = new Vector3(Random.Range(-0.75f, 0.75f), 0, Random.Range(-1.1f, 1.1f));
        var pos3 = new Vector3(Random.Range(-0.75f, 0.75f), 0, Random.Range(-1.1f, 1.1f));

        Instantiate(toys[index], instantiatePoint.position + pos1, Quaternion.identity);
        Instantiate(toys[index + 1], instantiatePoint.position + pos2, Quaternion.identity);
        Instantiate(toys[index + 2], instantiatePoint.position + pos3, Quaternion.identity);

        toysCounter++;
        yield return new WaitForSeconds(0.02f);
        if (toysCounter < selectedLevel.toysNumber)
            StartCoroutine(InstantiateToy(index));
        else
        {
            UIManager.Instance.ActivateStartButton(true);
            gameState = GameState.Rotate;
        }
    }

    [SkipRename]
    public void Collect()
    {
        foreach (var bodyPart in Player.Instance.bodyParts)
            bodyPart.gameObject.SetActive(false);

        gameState = GameState.Collect;
        AudioManager.Instance.Vibrate();
        AudioManager.Instance.PlaySound(AudioManager.Instance.click);
        Player.Instance.playerState = PlayerState.Descending;
        UIManager.Instance.ActivateStartButton(false);
    }

    public void SwitchCams()
    {
        Player.Instance.transform.position = new Vector3(0, selectedLevel.path.transform.position.y - 0.5f, 0f);
        wire.position = new Vector3(0, selectedLevel.path.transform.position.y - 0.25f, 0f);
        cams[0].SetActive(false);
        cams[1].SetActive(true);
        Player.Instance.collider.enabled = true;
    }

    public void SetSpeedRushMode()
    {
        ChangeEnvironment(colors[5], 1f);
        wire1.SetColor("_BaseColor", Color.white);
        wire2.SetColor("_BaseColor", Color.white);
        obstacle1.SetTexture("_BaseMap", greenTexture);
        obstacle2.SetTexture("_BaseMap", greenTexture);
        gameState = GameState.SpeedRush;
        Player.Instance.speed = Player.Instance.rushSpeed;
        StartCoroutine(EndSpeedRush());
        StartCoroutine(Player.Instance.SetUpSpeedRushEffect());
    }

    private IEnumerator EndSpeedRush()
    {
        yield return new WaitForSeconds(speedRushDuration);
        var l = levels.IndexOf(selectedLevel) + 1;
        ChangeEnvironment(colors[SelectColor(l)], 1f);
        wire1.SetColor("_BaseColor", wireColors[0]);
        wire2.SetColor("_BaseColor", wireColors[1]);
        obstacle1.SetTexture("_BaseMap", yellowTexture);
        obstacle2.SetTexture("_BaseMap", redTexture);
        UIManager.Instance.ResetSpeedSlider();
        gameState = GameState.Play;
        Player.Instance.speed = Player.Instance.initialSpeed;
    }

    private void Run()
    {
        Vector3 leftFootRot = Vector3.zero, rightFootRot = Vector3.zero;
        SwitchCams();
        foreach (var bodyPart in Player.Instance.bodyParts)
        {
            var rot = bodyPart.bone.transform.localEulerAngles;

            switch (bodyPart.bodyPartName)
            {
                case BodyPartName.LeftArm or BodyPartName.RightArm:
                    bodyPart.bone.transform.localEulerAngles = new Vector3(90, rot.y,
                        rot.z);
                    break;
                case BodyPartName.RightFoot:
                {
                    if (rightFootRot == Vector3.zero)
                        rightFootRot = bodyPart.bone.transform.localEulerAngles;
                    bodyPart.bone.transform.localEulerAngles = new Vector3(-90, 0, rightFootRot.z);
                    bodyPart.childBone.transform.localEulerAngles = new Vector3(-90, 0, 0);
                    break;
                }
                case BodyPartName.LeftFoot:
                {
                    if (leftFootRot == Vector3.zero)
                        leftFootRot = bodyPart.bone.transform.localEulerAngles;
                    bodyPart.bone.transform.localEulerAngles = new Vector3(-90, 0, leftFootRot.z);
                    bodyPart.childBone.transform.localEulerAngles = new Vector3(-90, 0, 0);
                    break;
                }
            }

            foreach (var sprite in Player.Instance.bodyParts)
                sprite.gameObject.SetActive(false);

            Player.Instance.playerState = PlayerState.Pausing;
        }
    }
}