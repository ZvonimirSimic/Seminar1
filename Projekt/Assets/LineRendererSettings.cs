using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Valve.VR;
using BNG;
using TMPro;

public class LineRendererSettings : MonoBehaviour
{
    [SerializeField] LineRenderer rendRight;

    Vector3[] points, originalScales, originalPositions;
    //string[] notes;
    Vector3 parentPosition, parentScale;
    Quaternion parentRotation;
    Quaternion[] originalRotations;
    bool objectCreated = false, startTimer = false, screenshot = false, record = false, recording = false;
    private int screenCapMode = 0;
    public Camera camera;
    private float lastTime, timerLastTime, lastTimeRecording;
    private List<GameObject> objectParts = new List<GameObject>();
    private List<GameObject> hiddenObjects = new List<GameObject>();

    private List<GameObject> crossSectionObjects = new List<GameObject>();
    private List<Material> originalMaterials = new List<Material>();

    //private MediaEncoder enc;
    private GameObject parentObject;
    private string currentSetting = "";
    private ColorBlock defaultBlock = new ColorBlock();
    public UnityEngine.UI.Button btn, lastBtn, planeButton, axesButton;
    //private Material originalMaterial;
    public Material crossSectionMaterial;
    public Image img;
    public GameObject selectedObject = null, noteText, noteInPanel, axes, axesPlanes, screenShotFrame;
    public GameObject panel, editPanel, scalePanel, rotationPanel, positionPanel, screenCapturePanel, infoPanel, gam;
    public LayerMask layerMask;
    private GameObject lastHitObject = null, parentInScene;
    public TMPro.TMP_Text countdownText;

    /*
    void hoverOut(bool state, GameObject newGameObject)
    {
        if (newGameObject != null)
        {
            if (lastHitObject == null)
            {
                lastHitObject = newGameObject;
                lastHitObject.GetComponent<Outline>().enabled = state;
            }
            else if (lastHitObject != newGameObject)
            {
                lastHitObject.GetComponent<Outline>().enabled = false;
                if (newGameObject != selectedObject)
                {
                    lastHitObject = newGameObject;
                    lastHitObject.GetComponent<Outline>().enabled = state;
                }
            }
        }
        else
        {
            if (lastHitObject != null)
            {
                lastHitObject.GetComponent<Outline>().enabled = state;
            }
            lastHitObject = null;
        }
    }
    */

    // Funkcija za osvjetljavanje obruba objekta kada se laserom preðe preko njega
    // newGameObject je objekt koji laser trenutno pogaða, a lastHitObject je onaj koji je pogaðao prije njega
    void hoverOut(bool state, GameObject newGameObject)
    {
        if (newGameObject != null)
        {
            if (lastHitObject == null)
            {
                lastHitObject = newGameObject;
                lastHitObject.GetComponent<Outline>().enabled = state;
            }
            else if (lastHitObject != newGameObject)
            {
                lastHitObject.GetComponent<Outline>().enabled = false;
                /*if (newGameObject != selectedObject)
                {*/
                    lastHitObject = newGameObject;
                    lastHitObject.GetComponent<Outline>().enabled = state;
               // }
            }
        }
        else
        {
            if (lastHitObject != null)
            {
                lastHitObject.GetComponent<Outline>().enabled = state;
            }
            lastHitObject = null;
        }
        if (selectedObject != null)
        {
            selectedObject.GetComponent<Outline>().enabled = true;
        }
    }

    // Funkcija za odabir objekta - ako je odabran neki objekt i pokuša se odabrati novi, stari prestaje biti odabran, a novi se odabire
    void selectObject(GameObject obj)
    {
        Debug.Log("Entered selectObject.");
        if (selectedObject != null)
        {
            selectedObject.GetComponent<Outline>().OutlineColor = Color.green;
            selectedObject.GetComponent<Outline>().enabled = false;
        }
        /*if (selectedObject != obj)
        {*/
            selectedObject = obj;
            selectedObject.GetComponent<Outline>().OutlineColor = Color.red;
            selectedObject.GetComponent<Outline>().enabled = true;
        /*}
        else if(selectedObject == obj && obj != null && selectedObject != null)
        {
            selectedObject.GetComponent<Outline>().OutlineColor = Color.green;
            selectedObject.GetComponent<Outline>().enabled = false;
            selectedObject = null;
        }*/
    }

    // Funkcija s kojom namještamo laser da se zaustavi i promjeni boju kada zakljuèi da je pogodio neki collider
    // Ako se stisne prekidaè dok laser pogaða objekt, taj objekt se odabire te dobiva crveni obrub koji ostaje upaljen dok god je objekt odabran
    // Odabrati se može samo objekt koji sadrži komponentu obruba
    // Osim odabira objekta, može se odabrati i objekt koji je gumb pa se onda izvršava ono što gumb odredi
    public bool AlignRightLineRenderer(LineRenderer rendRight)
    {
        /*Ray ray;
        ray = new Ray(rendRight.transform.position, rendRight.transform.forward);*/
        RaycastHit hit;
        bool btnHit = false;

        if (Physics.Raycast(rendRight.transform.position, rendRight.transform.forward, out hit, 20))
        {
            if (hit.collider.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
            {
                btn = hit.collider.gameObject.GetComponentInParent<UnityEngine.UI.Button>();
            }
            //Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject.GetComponent<Outline>() != null)
            {
                if(BNG.ControllerBinding.RightTrigger.GetDown())
                {
                    selectObject(hit.collider.gameObject);
                }
                hoverOut(true, hit.collider.gameObject);
            }
            else
            {
                hoverOut(false, null);
            }
            rendRight.useWorldSpace = true;
            rendRight.SetPosition(0, rendRight.transform.position);
            rendRight.SetPosition(1, hit.point);
            rendRight.startColor = Color.green;
            rendRight.endColor = Color.green;
            btnHit = true;
        }
        else
        {
            hoverOut(false, null);
            rendRight.useWorldSpace = false;
            rendRight.SetPosition(0, rendRight.transform.localPosition);
            rendRight.SetPosition(1, new Vector3(0, 0, 20));
            rendRight.startColor = Color.red;
            rendRight.endColor = Color.red;
            btnHit = false;
        }
        rendRight.material.color = rendRight.startColor;
        return btnHit;
    }

    // Postavljaju se poèetne vrijednosti lasera
    void Start()
    {
        setDefaultBlock();
        lastTime = Time.time;
        timerLastTime = Time.time;
        lastTimeRecording = Time.time;
        //rendRight = rendRight.gameObject.GetComponent<LineRenderer>();
        img = panel.GetComponent<Image>();
        points = new Vector3[2];
        points[0] = Vector3.zero;
        points[1] = rendRight.transform.position + new Vector3(0, 0, 20);
        rendRight.SetPositions(points);
        rendRight.enabled = true;
    }

    // Funkcija za kreaciju objekta unutar scene
    // Prije nego se objekt stvori u sceni, dobiva potrebne komponente i pamte se originalne vrijednosti rotacije, pozicije i velièine objekta
    // Svaki objekt dobiva objekt roditelja te se ne stvara objekt po objekt nego se roditeljski objekt stvori zbog èega se stvore svi ostali objekti
    void createObject()
    {
        GameObject desk = GameObject.FindGameObjectWithTag("Desk");
        Vector3 newPos = new Vector3(desk.transform.position.x, desk.transform.position.y + 0.5F, desk.transform.position.z);
        gam.transform.position = newPos;
        Transform[] children = gam.GetComponentsInChildren<Transform>(true);

        List<GameObject> childrenList = new List<GameObject>();
        //List<GameObject> textFields = new List<GameObject>();
        List<GameObject> gamObjs = new List<GameObject>();
        for (int i = 0; i < children.Length; ++i)
        {
            GameObject g = new GameObject();
            /*GameObject t = new GameObject();
            textFields.Add(t);*/
            gamObjs.Add(g);
            Transform child = children[i];
            childrenList.Add(child.gameObject);
        }

        parentObject = children[0].gameObject;
        //gamObjs[0] = Instantiate(parentObject, newPos + parentObject.transform.localPosition / 50, parentObject.transform.rotation);

        Debug.Log("Children list count: " + childrenList.Count);

        originalScales = new Vector3[childrenList.Count];
        originalPositions = new Vector3[childrenList.Count];
        originalRotations = new Quaternion[childrenList.Count];
        //notes = new string[childrenList.Count];

        for (int i = 1; i < childrenList.Count; i++)
        {
            if (childrenList[i].transform.childCount <= 0)
            {
                if (childrenList[i].GetComponent<Grabbable>() == null)
                {
                    childrenList[i].AddComponent<Grabbable>();
                    childrenList[i].GetComponent<Grabbable>().SecondaryGrabBehavior = OtherGrabBehavior.DualGrab;
                    childrenList[i].GetComponent<Grabbable>().GrabMechanic = GrabType.Precise;
                    childrenList[i].GetComponent<Grabbable>().GrabButton = GrabButton.Grip;
                    childrenList[i].GetComponent<Grabbable>().GrabPhysics = GrabPhysics.Kinematic;
                    childrenList[i].GetComponent<Grabbable>().Grabtype = HoldType.HoldDown;
                    childrenList[i].GetComponent<Grabbable>().HideHandGraphics = true;
                    childrenList[i].GetComponent<Grabbable>().ParentToHands = true;
                    childrenList[i].GetComponent<Grabbable>().ParentHandModel = false;
                    childrenList[i].GetComponent<Grabbable>().SnapHandModel = true;
                }
                if (childrenList[i].GetComponent<Rigidbody>() == null)
                {
                    childrenList[i].AddComponent<Rigidbody>();
                    childrenList[i].GetComponent<Rigidbody>().isKinematic = true;
                    childrenList[i].GetComponent<Rigidbody>().useGravity = true;
                }
                if (childrenList[i].GetComponent<MeshCollider>() == null)
                {
                    childrenList[i].AddComponent<MeshCollider>();
                }
                /*if (childrenList[i].GetComponent<TextMeshPro>() == null)
                {
                    childrenList[i].AddComponent<TextMeshPro>();
                }*/

                /* if (textFields[i].GetComponent<TextMeshPro>() == null)
                 {
                     textFields[i].AddComponent<TextMeshPro>();
                 }*/
                if (childrenList[i].GetComponent<Outline>() == null)
                {
                    childrenList[i].AddComponent<Outline>();
                    childrenList[i].GetComponent<Outline>().OutlineColor = Color.green;
                    childrenList[i].GetComponent<Outline>().enabled = false;
                }
                //childrenList[i] = Instantiate(childrenList[i], newPos + childrenList[i].transform.localPosition, childrenList[i].transform.rotation);
                //childrenList[i].transform.localScale = new Vector3(gamObjs[i].transform.localScale.x, gamObjs[i].transform.localScale.y, gamObjs[i].transform.localScale.z);
                Debug.Log("Object " + i + ": " + childrenList[i].name);

                originalPositions[i] = childrenList[i].transform.localPosition;
                originalRotations[i] = childrenList[i].transform.localRotation;
                originalScales[i] = childrenList[i].transform.localScale;

                childrenList[i].transform.SetParent(parentObject.transform);
                /*textFields[i].transform.parent = childrenList[i].transform;
                float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
                textFields[i].transform.localScale /= 30;
                textFields[i].GetComponent<TextMeshPro>().transform.position = new Vector3(gamObjs[i].transform.position.x, gamObjs[i].transform.position.y - gamObjs[i].transform.localPosition.y, gamObjs[i].transform.position.z);*/

                //objectParts = childrenList;

                objectParts.Add(childrenList[i]);
            }
        }
        /*parentObject.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        parentObject.transform.localPosition -= new Vector3(0, 0, 2);*/
        parentInScene = Instantiate(parentObject, newPos + parentObject.transform.localPosition, parentObject.transform.rotation);
        parentPosition = parentInScene.transform.localPosition;
        parentRotation = parentInScene.transform.localRotation;
        parentScale = parentInScene.transform.localScale;

        /*GameObject childrenListNew = new GameObject();
        Transform[] parentChildren = parentInScene.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < parentChildren.Length; ++i)
        {
            originalPositions[i] = parentChildren[i].transform.localPosition;
            originalRotations[i] = parentChildren[i].transform.localRotation;
            originalScales[i] = parentChildren[i].transform.localScale;
        }*/
    }
    
    // Uništavaju se objekti te se prazni lista skrivenih objekata
    void destroyObject()
    {
        for(int i = 0; i < objectParts.Count; i++)
        {
            //Destroy(objectParts[i], 1.0f);
            Destroy(parentInScene, 1.0f);
            hiddenObjects = new List<GameObject>();
        }
    }

    // Funkcija koja upravlja gumbima prostora za presjek objekta
    // PlanesButton može sakriti (ili pokazati) prozirne ravnine kod prostora za presjek
    // AxesButton može sakriti (ili pokazati) koordinatne osi kod prostora za presjek
    public void crossSectionMenuSettings()
    {
        if(btn.name == "PlanesButton")
        {
            if (planeButton.GetComponentInChildren<TextMeshProUGUI>().text == "On")
            {
                planeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Off";
                ColorBlock cb = defaultBlock;
                cb.normalColor = Color.cyan;
                planeButton.colors = cb;
                MeshRenderer[] renderers = axesPlanes.GetComponentsInChildren<MeshRenderer>();
                for(int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
            }
            else
            {
                planeButton.GetComponentInChildren<TextMeshProUGUI>().text = "On";
                planeButton.colors = defaultBlock;
                MeshRenderer[] renderers = axesPlanes.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = true;
                }
            }
        }
        else if(btn.name == "AxesButton")
        {
            if (axesButton.GetComponentInChildren<TextMeshProUGUI>().text == "On")
            {
                axesButton.GetComponentInChildren<TextMeshProUGUI>().text = "Off";
                ColorBlock cb = defaultBlock;
                cb.normalColor = Color.cyan;
                axesButton.colors = cb;
                MeshRenderer[] renderers = axes.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
            }
            else
            {
                axesButton.GetComponentInChildren<TextMeshProUGUI>().text = "On";
                axesButton.colors = defaultBlock;
                MeshRenderer[] renderers = axes.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = true;
                }
            }
        }
        btn = null;
    }

    // Funkcija koja upravlja gumbima glavnog menua
    public void menuSettings()
    {
        if (btn != null)
        {
            if (btn.name == "Resume")  // gasi menu
            {
                if (panel.active)
                {
                    panel.SetActive(false);
                    Debug.Log("Menu closed.");
                }
            }
            else if (btn.name == "EditObject") // otvara menu za ureðivanje objekta
            {
                if (objectCreated == true)
                {
                    panel.SetActive(false);

                    editPanel.SetActive(true);
                    Debug.Log("Menu closed.");
                }
            }
            else if (btn.name == "CaptureScreen") // otvara menu za snimanje zaslona
            {
                panel.SetActive(false);
                screenCapturePanel.SetActive(true);
            }
            else if (btn.name == "ShowHidden") // pokazuje sve skrivene objekte
            {
                /*for(int i = 0; i < objectParts.Capacity; i++)
                {
                    if (objectParts[i].GetComponent<MeshRenderer>())
                    {
                        Debug.Log("Objekt " + i + ". : " + objectParts[i].name + " " + objectParts[i].GetComponent<MeshRenderer>().enabled);
                        objectParts[i].GetComponent<MeshRenderer>().enabled = true;
                        Debug.Log("Objekt " + i + ". : " + objectParts[i].name + " " + objectParts[i].GetComponent<MeshRenderer>().enabled);
                    }
                }*/
                for (int i = 0; i < hiddenObjects.Count; i++)
                {
                    if (hiddenObjects[i].GetComponent<MeshRenderer>())
                    {
                        hiddenObjects[i].GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                hiddenObjects = new List<GameObject>();
            }
            else if (btn.name == "SelectNone") // odznaèuje odabrani objekt
            {
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<Outline>().OutlineColor = Color.green;
                    selectedObject.GetComponent<Outline>().enabled = false;
                    selectedObject = null;
                }
            }
            else if (btn.name == "CreateObject") // kreira objekt
            {
                if (!objectCreated)
                {
                    createObject();
                    objectCreated = true;
                }
                else
                {
                    destroyObject();
                    createObject();
                    selectedObject = null;
                }
            }
            else if (btn.name == "Exit") // gasi aplikaciju
            {
                Debug.Log("Application closed.");
                Application.Quit();
            }
        }
        btn = null;
    }

    // Funkcija koja upravlja gumbina menua za snimanje zaslona
    public void captureMenu()
    {
        if (btn != null)
        {
            if (btn.name == "CaptureResume")  // gasi menu za snimanje zaslona
            {
                screenCapturePanel.SetActive(false);
            }
            else if (btn.name == "StartRecording")
            {
                /*screenShotFrame.SetActive(true);
                if (recording == true)
                {
                    recording = false;
                    enc.Dispose();
                    screenShotFrame.SetActive(false);
                }
                else
                {
                    screenCapturePanel.SetActive(false);
                    var vidAttr = new VideoTrackAttributes
                    {
                        bitRateMode = VideoBitrateMode.Medium,
                        frameRate = new MediaRational(25),
                        width = 1920,
                        height = 1080,
                        includeAlpha = false
                    };

                    var audAttr = new AudioTrackAttributes
                    {
                        sampleRate = new MediaRational(48000),
                        channelCount = 2
                    };
                    Debug.Log("Creating mp4 file");
                    enc = new MediaEncoder("sample.mp4", vidAttr, audAttr);
                    Debug.Log("Created mp4 file");
                    startTimer = true;
                    screenCapMode = 1;
                }*/
            }
            else if (btn.name == "TakeScreenshot")
            {
                screenShotFrame.SetActive(true);
                Debug.Log("pressed take screenshot");
                screenCapturePanel.SetActive(false);
                Debug.Log("next is takescreenshot");
                startTimer = true;
                screenCapMode = 2;
            }
            else if (btn.name == "CaptureBack") // vraæa se na glavni menu
            {
                screenCapturePanel.SetActive(false);
                panel.SetActive(true);
            }
        }
        btn = null;
    }

    IEnumerator takeScreenshot()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("In takeScreenshot");
        int width = Screen.width, height = Screen.height, i = 1;
        Debug.Log("Width: " + width + " Height: " + height);
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Rect rect = new Rect(0,0,width,height);
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();
        byte[] byteArray = texture.EncodeToJPG();
        while(i < 100)
        {
            string file = Application.dataPath + "/../" + "/Screenshot" + i + ".jpg";
            if (!System.IO.File.Exists(file)){
                System.IO.File.WriteAllBytes(file, byteArray);
                Debug.Log("Saved file to: " + file);
                break;
            }
            i++;
        }
    }

    /*IEnumerator addScreenshot() //mozda ovo promijeniti da nije ienumerator?
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("In takeScreenshot");
        int width = Screen.width, height = Screen.height;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Rect rect = new Rect(0, 0, width, height);
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();
        enc.AddFrame(texture);
    }*/
    
    void screenCaptureDelay()
    {
        Debug.Log("In screencapturedelay");

        if (countdownText.text == "")
        {
            if(screenshot == true && screenCapMode == 2)
            {
                StartCoroutine(takeScreenshot());
                startTimer = false;
                screenshot = false;
                screenCapMode = 0;
                if (recording != true)
                {
                    screenShotFrame.SetActive(false);
                }
            }
            else if(record == true && screenCapMode == 1)
            {
                //Debug.Log("Recording is : " + recording);
                startTimer = false;
                record = false;
                recording = true;
                screenCapMode = 0;
            }
            else
            {
                countdownText.text = "3";
            }
        }
        else if (countdownText.text == "3")
        {
            countdownText.text = "2";
        }
        else if (countdownText.text == "2")
        {
            countdownText.text = "1";
        }
        else if (countdownText.text == "1")
        {
            countdownText.text = "";
            if (screenCapMode == 2)
            {
                screenshot = true;
            }
            else if (screenCapMode == 1)
            {
                record = true;
            }
            //Debug.Log("Recording is : " + recording);

        }
    }

    // Funkcija koja upravlja gumbima menua s informacijama
    public void infoMenuSettings()
    {
        if (btn != null)
        {
            if (btn.name == "InfoResume") // gasi menu s informacijama
            {
                infoPanel.SetActive(false);
            }
            else if (btn.name == "InfoBack") // gasu menu s informacijama i pali menu za ureðivanje
            {
                infoPanel.SetActive(false);
                editPanel.SetActive(true);
            }
        }
        btn = null;
    }

    // Funkcija koja upravlja podacima koji se ispisuju u prozoru s informacijama
    private void infoMenuValues()
    {
        /*Debug.Log("Length: " + selectedObject.GetComponent<MeshFilter>().mesh.bounds.extents.x);
        Debug.Log("Height: " + selectedObject.GetComponent<MeshFilter>().mesh.bounds.extents.y);
        Debug.Log("Width: " + selectedObject.GetComponent<MeshFilter>().mesh.bounds.extents.z);*/
        float x = selectedObject.GetComponent<MeshFilter>().mesh.bounds.size.x * selectedObject.transform.localScale.x * parentInScene.transform.localScale.x,
            y= selectedObject.GetComponent<MeshFilter>().mesh.bounds.size.y * selectedObject.transform.localScale.y * parentInScene.transform.localScale.y,
            z= selectedObject.GetComponent<MeshFilter>().mesh.bounds.size.z * selectedObject.transform.localScale.z * parentInScene.transform.localScale.z;
        /*Debug.Log("Diameter: " + Mathf.Sqrt(Mathf.Pow(x,2) + Mathf.Pow(y,2) + Mathf.Pow(z,2)));
        Debug.Log("Angle x: " + selectedObject.transform.localRotation.eulerAngles.x + "°");
        Debug.Log("Angle y: " + selectedObject.transform.localRotation.eulerAngles.y + "°");
        Debug.Log("Angle z: " + selectedObject.transform.localRotation.eulerAngles.z + "°");*/
        Transform[] children = infoPanel.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if(child.gameObject.name == "Length")
            {
                child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Length: " + x;
            }
            else if(child.gameObject.name == "Height")
            {
                child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Height: " + y;
            }
            else if (child.gameObject.name == "Width")
            {
                child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Width: " + z;
            }
            else if (child.gameObject.name == "Diameter")
            {
                child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Diameter: " + Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2) + Mathf.Pow(z, 2));
            }
            else if (child.gameObject.name == "AngleX")
            {
                child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Angle x: " + selectedObject.transform.localRotation.eulerAngles.x + "°";
            }
            else if (child.gameObject.name == "AngleY")
            {
                child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Angle y: " + selectedObject.transform.localRotation.eulerAngles.y + "°";
            }
            else if (child.gameObject.name == "AngleZ")
            {
                child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Angle z: " + selectedObject.transform.localRotation.eulerAngles.z + "°";
            }
        }
    }

    // Funkcija koja upravlja gumbima menua za ureðivanje
    public void editSettings()
    {
        if (btn != null)
        {
            if (btn.name == "Resume") // gasi menu za ureðivanje
            {
                editPanel.SetActive(false);
                Debug.Log("Edit menu closed.");
            }
            else if (btn.name == "ObjectInfo") // otvara menu s informacijama
            {
                if (selectedObject != null)
                {
                    editPanel.SetActive(false);
                    infoPanel.SetActive(true);
                    infoMenuValues();
                }
            }
            else if (btn.name == "SetNote") // postavlja zabilješku na objekt
            {
                if (selectedObject != null)
                {
                    if (selectedObject.GetComponent<UnityEngine.UI.Text>() != null)
                    {
                        selectedObject.GetComponentInChildren<UnityEngine.UI.Text>().text = noteText.GetComponentInChildren<VRKeys.Keyboard>().text;
                        Debug.Log("Set new text on obj: " + selectedObject.GetComponentInChildren<UnityEngine.UI.Text>().text);
                    }
                    else
                    {
                        selectedObject.AddComponent<UnityEngine.UI.Text>();
                        selectedObject.GetComponentInChildren<UnityEngine.UI.Text>().text = noteText.GetComponentInChildren<VRKeys.Keyboard>().text;
                        Debug.Log("Added new text on obj: " + selectedObject.GetComponentInChildren<UnityEngine.UI.Text>().text);
                    }
                }
            }
            else if (btn.name == "CrossSection") // dodaje objektu moguænost korištenja presjeka
            {
                if (selectedObject != null)
                {
                    if (selectedObject.GetComponent<Renderer>().material.name == (crossSectionMaterial.name + " (Instance)"))
                    {
                        //Debug.Log("Object material set to: " + selectedObject.GetComponent<Renderer>().material.name);
                        for (int i = 0; i < crossSectionObjects.Count; i++)
                        {
                            if (selectedObject.name == crossSectionObjects[i].name)
                            {
                                selectedObject.GetComponent<Renderer>().material = originalMaterials[i];
                                originalMaterials.RemoveAt(i);
                                crossSectionObjects.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        originalMaterials.Add(selectedObject.GetComponent<Renderer>().material);
                        crossSectionObjects.Add(selectedObject);
                        selectedObject.GetComponent<Renderer>().material = crossSectionMaterial;
                    }
                }
            }
            else if (btn.name == "OriginalMaterials") // svim objektima uklanja moguænost korištenja presjeka
            {
                for (int i = 0; i < crossSectionObjects.Count; i++)
                {
                    crossSectionObjects[i].GetComponent<Renderer>().material = originalMaterials[i];
                }
                crossSectionObjects = new List<GameObject>();
                originalMaterials = new List<Material>();
            }
            else if (btn.name == "HideObject") // skriva objekt
            {
                if (selectedObject != null)
                {
                    /*for (int i = 0; i < objectParts.Count; i++)
                    {
                        Debug.Log("ObjectParts : " + objectParts.Count);
                        if (selectedObject.name == objectParts[i].name)
                        {
                            /*objectParts[i].GetComponent<Outline>().OutlineColor = Color.green;
                            objectParts[i].GetComponent<Outline>().enabled = false;
                            objectParts[i].GetComponentInChildren<MeshRenderer>().enabled = false;
                            Debug.Log("Objekt " + i + ". : " + objectParts[i].name + " " + objectParts[i].GetComponentInChildren<MeshRenderer>().enabled);*/
                    selectedObject.GetComponent<Outline>().OutlineColor = Color.green;
                    selectedObject.GetComponent<Outline>().enabled = false;
                    selectedObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                    hiddenObjects.Add(selectedObject);
                    selectedObject = null;
                    //break;
                    // }
                    //}
                }
            }
            else if (btn.name == "ScaleObject") // otvara menu za promjenu velièine objekta
            {
                editPanel.SetActive(false);
                scalePanel.transform.parent.gameObject.transform.parent.gameObject.SetActive(true);
                scalePanel.transform.parent.gameObject.transform.parent.gameObject.transform.position = panel.transform.position;
            }
            else if (btn.name == "RepositionObject") // otvara menu za promjenu pozicije objekta
            {
                editPanel.SetActive(false);
                positionPanel.transform.parent.gameObject.transform.parent.gameObject.SetActive(true);
                positionPanel.transform.parent.gameObject.transform.parent.gameObject.transform.position = panel.transform.position;
            }
            else if (btn.name == "RotateObject") // otvara menu za promjenu rotacije objekta
            {
                editPanel.SetActive(false);
                rotationPanel.transform.parent.gameObject.transform.parent.gameObject.SetActive(true);
                rotationPanel.transform.parent.gameObject.transform.parent.gameObject.transform.position = panel.transform.position;
            }
            else if (btn.name == "Back") // gasi menu za ureðivanje i pali glavni menu
            {
                editPanel.SetActive(false);
                panel.SetActive(true);
            }
        }
        btn = null;
    }

    // Funkcija za postavljanje bijelog gumba
    private void setDefaultBlock()
    {
        defaultBlock.normalColor = Color.white;
        defaultBlock.highlightedColor = Color.white;
        defaultBlock.fadeDuration = 0.1f;
        defaultBlock.colorMultiplier = 1;
        defaultBlock.pressedColor = Color.white;
        defaultBlock.disabledColor = Color.white;
        defaultBlock.selectedColor = Color.white;
    }

    // Funkcija s kojom se odabire koje vrijednosti æe se mijenjati na objektu kada stišæemo gumb
    private void chooseSetting()
    {
        if (currentSetting != "" && btn.name == lastBtn.name)
        {
            currentSetting = "";
            lastBtn.colors = defaultBlock;
        }
        else
        {
            if (currentSetting != "")
            {
                lastBtn.colors = defaultBlock;
            }
            currentSetting = btn.name;                  // Nullreference exception
            ColorBlock cb = defaultBlock;
            cb.normalColor = Color.cyan;
            btn.colors = cb;
            lastBtn = btn;
        }
    }

    // Funkcija koja uptavlja menuom za skaliranje objekta
    public void scaleSettings()
    {
        if (btn != null)
        {
            if (btn.name == "SizeClose")
            {
                scalePanel.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
            }
            else if (btn.name == "OriginalSize")
            {
                if (selectedObject != null)
                {
                    for (int i = 0; i < objectParts.Count; i++)
                    {
                        if (selectedObject.name == objectParts[i].name)
                        {
                            selectedObject.transform.localScale = originalScales[i+1];
                            Debug.Log("Original size: " + originalScales[i].x + ", " + originalScales[i].y + ", " + originalScales[i].z);
                            break;
                        }
                    }
                }
                else
                {
                    parentInScene.transform.localScale = parentScale;
                    for (int i = 1; i <= objectParts.Count; i++)
                    {
                        parentInScene.transform.GetChild(i - 1).transform.localScale = originalScales[i];
                    }
                }
            }
            else if (btn.name == "AddVerySmallXScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0.01f, 0f, 0f);
                }
            }
            else if (btn.name == "SubtractVerySmallXScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0.01f, 0f, 0f);
                }
            }
            else if (btn.name == "AddSmallXScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0.1f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0.1f, 0f, 0f);
                }
            }
            else if (btn.name == "SubtractSmallXScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0.1f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0.1f, 0f, 0f);
                }
            }
            else if (btn.name == "AddMediumXScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(1f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(1f, 0f, 0f);
                }
            }
            else if (btn.name == "SubtractMediumXScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(1f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(1f, 0f, 0f);
                }
            }
            else if (btn.name == "AddVerySmallYScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 0.01f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 0.01f, 0f);
                }
            }
            else if (btn.name == "SubtractVerySmallYScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 0.01f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 0.01f, 0f);
                }
            }
            else if (btn.name == "AddSmallYScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 0.1f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 0.1f, 0f);
                }
            }
            else if (btn.name == "SubtractSmallYScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 0.1f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 0.1f, 0f);
                }
            }
            else if (btn.name == "AddMediumYScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 1f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 1f, 0f);
                }
            }
            else if (btn.name == "SubtractMediumYScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 1f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 1f, 0f);
                }
            }
            else if (btn.name == "AddVerySmallZScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 0f, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 0f, 0.01f);
                }
            }
            else if (btn.name == "SubtractVerySmallZScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 0f, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 0f, 0.01f);
                }
            }
            else if (btn.name == "AddSmallZScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 0f, 0.1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 0f, 0.1f);
                }
            }
            else if (btn.name == "SubtractSmallZScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 0f, 0.1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 0f, 0.1f);
                }
            }
            else if (btn.name == "AddMediumZScale")
            {
                if(selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 0f, 1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 0f, 1f);
                }
            }
            else if (btn.name == "SubtractMediumZScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 0f, 1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 0f, 1f);
                }
            }
            else if (btn.name == "AddVerySmallAllScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0.01f / parentInScene.transform.lossyScale.y, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else if (btn.name == "SubtractVerySmallAllScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0.01f / parentInScene.transform.lossyScale.y, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else if (btn.name == "AddSmallAllScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0.1f / parentInScene.transform.lossyScale.x, 0.1f / parentInScene.transform.lossyScale.y, 0.1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
            else if (btn.name == "SubtractSmallAllScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0.1f / parentInScene.transform.lossyScale.x, 0.1f / parentInScene.transform.lossyScale.y, 0.1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
            else if (btn.name == "AddMediumAllScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(1f / parentInScene.transform.lossyScale.x, 1f / parentInScene.transform.lossyScale.y, 1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(1f, 1f, 1f);
                }
            }
            else if (btn.name == "SubtractMediumAllScale")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(1f / parentInScene.transform.lossyScale.x, 1f / parentInScene.transform.lossyScale.y, 1f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(1f, 1f, 1f);
                }
            }
        }
        btn = null;
    }

    // Funkcija koja uptavlja menuom za rotaciju objekta
    public void rotationSettings()
    {
        if (btn != null)
        {
            if (btn.name == "RotationClose")
            {
                rotationPanel.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
            }
            else if (btn.name == "OriginalRotation")
            {
                if (selectedObject != null)
                {
                    for (int i = 0; i < objectParts.Count; i++)
                    {
                        if (selectedObject.name == objectParts[i].name)
                        {
                            selectedObject.transform.localRotation = originalRotations[i+1];
                            break;
                        }
                    }
                }
                else
                {
                    parentInScene.transform.localRotation = parentRotation;
                    for(int i = 1; i <= objectParts.Count; i++)
                    {
                        parentInScene.transform.GetChild(i-1).transform.localRotation = originalRotations[i];
                    }
                }
            }
            else if (btn.name == "AddVerySmallXRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0.1f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0.1f, 0, 0);
                }
            }
            else if (btn.name == "SubtractVerySmallXRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-0.1f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-0.1f, 0, 0);
                }

            }
            else if (btn.name == "AddSmallXRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(1f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(1f, 0, 0);
                }
            }
            else if (btn.name == "SubtractSmallXRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-1f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-1f, 0, 0);
                }
            }
            else if (btn.name == "AddMediumXRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(10f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(10f, 0, 0);
                }
            }
            else if (btn.name == "SubtractMediumXRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-10f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-10f, 0, 0);
                }
            }
            else if (btn.name == "AddVerySmallYRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0.1f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0.1f, 0);
                }
            }
            else if (btn.name == "SubtractVerySmallYRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, -0.1f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, -0.1f, 0);
                }
            }
            else if (btn.name == "AddSmallYRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 1f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 1f, 0);
                }
            }
            else if (btn.name == "SubtractSmallYRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, -1f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, -1f, 0);
                }
            }
            else if (btn.name == "AddMediumYRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 10f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 10f, 0);
                }
            }
            else if (btn.name == "SubtractMediumYRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, -10f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, -10f, 0);
                }
            }
            else if (btn.name == "AddVerySmallZRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, 0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, 0.1f);
                }
            }
            else if (btn.name == "SubtractVerySmallZRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, -0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, -0.1f);
                }
            }
            else if (btn.name == "AddSmallZRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, 1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, 1f);
                }
            }
            else if (btn.name == "SubtractSmallZRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, -1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, -1f);
                }
            }
            else if (btn.name == "AddMediumZRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, 10f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, 10f);
                }
            }
            else if (btn.name == "SubtractMediumZRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, -10f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, -10f);
                }
            }
            else if (btn.name == "AddVerySmallAllRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0.1f, 0.1f, 0.1f);
                }
            }
            else if (btn.name == "SubtractVerySmallAllRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-0.1f, -0.1f, -0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-0.1f, -0.1f, -0.1f);
                }
            }
            else if (btn.name == "AddSmallAllRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(1f, 1f, 1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(1f, 1f, 1f);
                }
            }
            else if (btn.name == "SubtractSmallAllRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-1f, -1f, -1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-1f, -1f, -1f);
                }
            }
            else if (btn.name == "AddMediumAllRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(10f, 10f, 10f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(10f, 10f, 10f);
                }
            }
            else if (btn.name == "SubtractMediumAllRotation")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-10f, -10f, -10f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-10f, -10f, -10f);
                }
            }
        }
        btn = null;
    }

    // Funkcija koja uptavlja menuom za pozicioniranje objekta
    public void positionSettings()
    {
        if (btn != null)
        {
            if (btn.name == "PositionClose")
            {
                positionPanel.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
            }
            else if (btn.name == "OriginalPosition")
            {
                if (selectedObject != null)
                {
                    for (int i = 0; i < objectParts.Count; i++)
                    {
                        if (selectedObject.name == objectParts[i].name)
                        {
                            selectedObject.transform.localPosition = originalPositions[i+1];
                            break;
                        }
                    }
                }
                else
                {
                    parentInScene.transform.localPosition = parentPosition;
                    for (int i = 1; i <= objectParts.Count; i++)
                    {
                        parentInScene.transform.GetChild(i - 1).transform.localPosition = originalPositions[i];
                    }
                }
            }
            else if (btn.name == "AddVerySmallXPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0.01f, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0.01f, 0f, 0f);
                }
            }
            else if (btn.name == "SubtractVerySmallXPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0.01f, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0.01f, 0f, 0f);
                }
            }
            else if (btn.name == "AddSmallXPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0.1f, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0.1f, 0f, 0f);
                }
            }
            else if (btn.name == "SubtractSmallXPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0.1f, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0.1f, 0f, 0f);
                }
            }
            else if (btn.name == "AddMediumXPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(1f, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(1f, 0f, 0f);
                }
            }
            else if (btn.name == "SubtractMediumXPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(1f, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(1f, 0f, 0f);
                }
            }
            else if (btn.name == "AddVerySmallYPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0f, 0.01f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0f, 0.01f, 0f);
                }
            }
            else if (btn.name == "SubtractVerySmallYPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0f, 0.01f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0f, 0.01f, 0f);
                }
            }
            else if (btn.name == "AddSmallYPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0f, 0.1f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0f, 0.1f, 0f);
                }
            }
            else if (btn.name == "SubtractSmallYPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0f, 0.1f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0f, 0.1f, 0f);
                }
            }
            else if (btn.name == "AddMediumYPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0f, 1f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0f, 1f, 0f);
                }
            }
            else if (btn.name == "SubtractMediumYPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0f, 1f, 0f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0f, 1f, 0f);
                }
            }
            else if (btn.name == "AddVerySmallZPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0f, 0f, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0f, 0f, 0.01f);
                }
            }
            else if (btn.name == "SubtractVerySmallZPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0f, 0f, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0f, 0f, 0.01f);
                }
            }
            else if (btn.name == "AddSmallZPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0f, 0f, 0.1f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0f, 0f, 0.1f);
                }
            }
            else if (btn.name == "SubtractSmallZPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0f, 0f, 0.1f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0f, 0f, 0.1f);
                }
            }
            else if (btn.name == "AddMediumZPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0f, 0f, 1f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0f, 0f, 1f);
                }
            }
            else if (btn.name == "SubtractMediumZPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0f, 0f, 1f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0f, 0f, 1f);
                }
            }
            else if (btn.name == "AddVerySmallAllPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0.01f, 0.01f, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else if (btn.name == "SubtractVerySmallAllPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0.01f, 0.01f, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else if (btn.name == "AddSmallAllPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
            else if (btn.name == "SubtractSmallAllPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
            else if (btn.name == "AddMediumAllPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(1f, 1f, 1f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(1f, 1f, 1f);
                }
            }
            else if (btn.name == "SubtractMediumAllPosition")
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(1f, 1f, 1f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(1f, 1f, 1f);
                }
            }
        }
        btn = null;
    }

    // Funkcija koja provjerava sadrži li objekt komentar te ako ga sadrži, upisuje ga u traku unutar menua za ureðivanje
    void checkNoteAgain()
    {
        if (editPanel.active && selectedObject != null && selectedObject.GetComponentInChildren<UnityEngine.UI.Text>() != null)
        {
            //Debug.Log("Text in inputfield = " + noteInPanel.GetComponent<TMP_InputField>().text);
            noteInPanel.GetComponent<TMP_InputField>().text = selectedObject.GetComponentInChildren<UnityEngine.UI.Text>().text;
        }
        else if(editPanel.active && selectedObject == null)
        {
            noteInPanel.GetComponent<TMP_InputField>().text = "";
        }
    }

    // Funkcija koja mijenja vrijednosti objektu ovisno o odabranom gumbu iz funkcije chooseSetting
    private void doSetting(bool operation)
    {
        if(currentSetting == "XAxisScale")
        {
            if(operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0.01f, 0f, 0f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0f, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0.01f, 0f, 0f);
                }
            }
        }
        else if (currentSetting == "YAxisScale")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 0.01f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 0.01f, 0f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 0.01f / parentInScene.transform.lossyScale.y, 0f);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 0.01f, 0f);
                }
            }
        }
        else if (currentSetting == "ZAxisScale")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0f, 0f, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0f, 0f, 0.01f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0f, 0f, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0f, 0f, 0.01f);
                }
            }
        }
        else if (currentSetting == "AllAxesScale")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale += new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0.01f / parentInScene.transform.lossyScale.y, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localScale -= new Vector3(0.01f / parentInScene.transform.lossyScale.x, 0.01f / parentInScene.transform.lossyScale.y, 0.01f / parentInScene.transform.lossyScale.z);
                }
                else
                {
                    parentInScene.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
        }
        else if (currentSetting == "XAxisRotation")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0.1f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0.1f, 0, 0);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-0.1f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-0.1f, 0, 0);
                }
            }
        }
        else if (currentSetting == "YAxisRotation")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0.1f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0.1f, 0);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, -0.1f, 0);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, -0.1f, 0);
                }
            }
        }
        else if (currentSetting == "ZAxisRotation")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, 0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, 0.1f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0, 0, -0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0, 0, -0.1f);
                }
            }
        }
        else if (currentSetting == "AllAxesRotation")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(0.1f, 0.1f, 0.1f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.localRotation *= Quaternion.Euler(-0.1f, -0.1f, -0.1f);
                }
                else
                {
                    parentInScene.transform.localRotation *= Quaternion.Euler(-0.1f, -0.1f, -0.1f);
                }
            }
        }
        if (currentSetting == "XAxisPosition")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0.01f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0.01f, 0, 0);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0.01f, 0, 0);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0.01f, 0, 0);
                }
            }
        }
        else if (currentSetting == "YAxisPosition")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0, 0.01f, 0);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0, 0.01f, 0);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0, 0.01f, 0);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0, 0.01f, 0);
                }
            }
        }
        else if (currentSetting == "ZAxisPosition")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0, 0, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0, 0, 0.01f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0, 0, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0, 0, 0.01f);
                }
            }
        }
        else if (currentSetting == "AllAxesPosition")
        {
            if (operation)
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position += new Vector3(0.01f, 0.01f, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition += new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
            else
            {
                if (selectedObject != null)
                {
                    selectedObject.transform.position -= new Vector3(0.01f, 0.01f, 0.01f);
                }
                else
                {
                    parentInScene.transform.localPosition -= new Vector3(0.01f, 0.01f, 0.01f);
                }
            }
        }
    }

    // Funkcija koja upravlja odabranim gumbima
    void checkCurrentSetting()
    {
        if (objectCreated && currentSetting != "")
        {
            Debug.Log("Objekt napravljen i setting izabran: " + currentSetting);
            if (BNG.ControllerBinding.AButton.GetDown() && (Time.time-lastTime>0.01f))  //povecaj
            {
                Debug.Log("Stisnut A");
                doSetting(true);
            }
            else if (BNG.ControllerBinding.BButton.GetDown() && (Time.time - lastTime > 0.01f))  //smanji
            {
                doSetting(false);
            }
            lastTime = Time.time;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if(recording == true && (Time.time - lastTimeRecording > 0.01f))
        {
            Debug.Log("In update for recording");
            StartCoroutine(addScreenshot());
            lastTimeRecording = Time.time;
        }*/
        if (startTimer == true && (Time.time - timerLastTime > 1.0f))
        {
            screenCaptureDelay();
            timerLastTime = Time.time;
        }
        if (infoPanel.active) // Ako je upaljen menu sa informacijama, provjeravaju se vrijednosti
        {
            infoMenuValues();
        }
        AlignRightLineRenderer(rendRight); // stalno se provjerava je li laser pogodio nešto
        checkCurrentSetting(); // ako je neki od gumba odabran, provjerava se ako želimo promijeniti velièinu
        if(AlignRightLineRenderer(rendRight) && BNG.ControllerBinding.RightTriggerDown.GetDown()) // ako je laser pogodio gumb i korisnik pritisnuo okidaè, gumb radi svoju funkciju
        {
            Debug.Log("Pressed");
            if (btn != null)
            {
                btn.onClick.Invoke();
            }
        }
        checkNoteAgain(); // Ako je upaljen menu za ureðivanje, provjerava se ima li objekt komentar
        btn = null;
    }
}