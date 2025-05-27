using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// This class is created for the example scene. There is no support for this script.
public class PlayerHealth : HealthManager
{
	public float health = 100f;
	public float criticalHealth = 30f;
	public Transform healthHUD;
	public AudioClip deathClip;
	public AudioClip[] hitClips;
	public GameObject hurtPrefab;
	public float decayFactor = 0.8f;

	private float totalHealth;
	
	private RectTransform healthBar, placeHolderBar;
	private Text healthLabel;
	private float originalBarScale;
	private bool critical;
	

	void Awake()
    {
		totalHealth = health;
		
		//healthBar = healthHUD.Find("HealthBar/Bar").GetComponent<RectTransform>();
		//placeHolderBar = healthHUD.Find("HealthBar/Placeholder").GetComponent<RectTransform>();
		//healthLabel = healthHUD.Find("HealthBar/Label").GetComponent<Text>();
		//originalBarScale = healthBar.sizeDelta.x;
		//healthLabel.text = "" + (int)health;
	}

	void Update()
	{
		
	}

	public bool IsFullLife()
	{
		return health >= totalHealth;
	}

	public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
	{
		health -= damage;

		UpdateHealthBar();

		

		if (health <= 0)
		{
			Kill();
		}
		else if (health <= criticalHealth && !critical)
		{
			critical = true;
			
		}

		//AudioSource.PlayClipAtPoint(hitClips[Random.Range(0, hitClips.Length)], location, 0.1f);
	}
	private void UpdateHealthBar()
	{
		//healthLabel.text = "" + (int)health;

		float scaleFactor = health / totalHealth;
		//healthBar.sizeDelta = new Vector2(scaleFactor * originalBarScale, healthBar.sizeDelta.y);
	}

	private void Kill()
	{
		dead = true;
		gameObject.layer = LayerMask.NameToLayer("Default");
		gameObject.tag = "Untagged";
		healthHUD.gameObject.SetActive(false);
		healthHUD.parent.Find("WeaponHUD").gameObject.SetActive(false);
		GetComponent<Animator>().SetBool("Aim", false);
		GetComponent<Animator>().SetBool("Cover", false);
		GetComponent<Animator>().SetFloat("Speed", 0);		
		AudioSource.PlayClipAtPoint(deathClip, transform.position, 5);
	}

}
