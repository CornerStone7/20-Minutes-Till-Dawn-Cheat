using flanne;
using flanne.Core;
using System.Reflection;
using UnityEngine;

namespace MinutesTillDawn
{
    class Hacks : MonoBehaviour
    {
        // Reflection parameters.
        BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        FieldInfo isPlayingField = null;
        FieldInfo isInvincibleField = null;
        FieldInfo isInfiniteField = null;

        // GameObjects.
        PlayerController localPlayerController = null;
        GunData copy_GunData;
        GameTimer gameTimer = null;
        GameObject pickupper = null;

        // Function Bool.
        private bool b_isPlaying;
        private bool b_isInvincibleFlipped = false;
        private bool b_isInfiniteFlipped = false;
        private int isInvincibleFlip;
        private int isInfiniteFlip;
        private bool b_isInit = false;

        // Hack Bool.
        private bool b_isInvincible = false;
        private bool b_PickUp = false;
        private bool b_KillAura = false;
        private bool b_InstantReload = false;
        private bool b_freezeMaxAmmo = false;
        private bool b_FireRate = false;
        private bool b_InfiniteAmmo = false;
        private bool b_ReRoll = false;

        public void OnGUI()
        {
            b_isInvincible = GUILayout.Toggle(b_isInvincible, "Invincible");
            b_KillAura = GUILayout.Toggle(b_KillAura, "Kill Aura");
            b_PickUp = GUILayout.Toggle(b_PickUp, "PickUp");
            b_InstantReload = GUILayout.Toggle(b_InstantReload, "Instant Reload");
            b_InfiniteAmmo = GUILayout.Toggle(b_InfiniteAmmo, "Infinite Ammo");
            b_freezeMaxAmmo = GUILayout.Toggle(b_freezeMaxAmmo, "Freeze Max Ammo");
            b_FireRate = GUILayout.Toggle(b_FireRate, "High Fire Rate");
            b_ReRoll = GUILayout.Toggle(b_ReRoll, "Enable Re Roll Passive");
        }
        public void Update()
        {
            // Init GameObjects.
            if (localPlayerController == null)
                localPlayerController = FindObjectOfType<PlayerController>();
            if (gameTimer == null)
                gameTimer = GameTimer.SharedInstance;
            if (pickupper == null)
                pickupper = GameObject.FindGameObjectWithTag("Pickupper");
            if (isPlayingField == null)
                isPlayingField = typeof(GameTimer).GetField("_isPlaying", bindFlags);
            if (localPlayerController != null)
            {
                if (isInvincibleField == null)
                    isInvincibleField = typeof(BoolToggle).GetField("_flip", bindFlags);
                if (isInfiniteField == null)
                    isInfiniteField = typeof(BoolToggle).GetField("_flip", bindFlags);
            }

            // Check if playing.
            b_isPlaying = (bool)isPlayingField.GetValue(gameTimer);
            // Check if invincible & infinite.
            if (localPlayerController != null)
            {
                isInvincibleFlip = (int)isInvincibleField.GetValue(localPlayerController.playerHealth.isInvincible);
                isInfiniteFlip = (int)isInfiniteField.GetValue(localPlayerController.ammo.infiniteAmmo);
            }

            // If Playing.
            if (b_isPlaying && localPlayerController != null)
            {
                // If not init.
                if (!b_isInit)
                {
                    copy_GunData = GameObject.Instantiate(localPlayerController.gun.gunData); // Copy current gun data.
                    b_isInit = true;
                }
                // If init.
                else if (b_isInit)
                {
                    // Hack Functions.
                    // God Mode.
                    if (b_isInvincible && isInvincibleFlip <= 0) // if Invincible duration is less than 0.
                    {
                        localPlayerController.playerHealth.isInvincible.Flip();
                        b_isInvincibleFlipped = true;
                    }
                    else if (!b_isInvincible && b_isInvincibleFlipped && isInvincibleFlip > 0) // if Invincible duration more than 0 & is increased by our hack.
                    {
                        localPlayerController.playerHealth.isInvincible.UnFlip();
                        b_isInvincibleFlipped = false;
                    }

                    // Kill Aura.
                    if (b_KillAura)
                        foreach (Health health in FindObjectsOfType<Health>())
                            health.TakeDamage(DamageType.Bullet, health.HP);

                    // Global Pickup Range.
                    if (b_PickUp)
                        pickupper.transform.localScale = Vector3.one * 100f;

                    // Instant Reload.
                    if (b_InstantReload)
                        localPlayerController.gun.gunData.reloadDuration = 0;
                    else if (!b_InstantReload)
                        localPlayerController.gun.gunData.reloadDuration = copy_GunData.reloadDuration;
                    
                    // Infinite Ammo.
                    if (b_InfiniteAmmo && isInfiniteFlip <= 0)  // if infiniteAmmo duration less than 0.
                    {
                        localPlayerController.ammo.infiniteAmmo.Flip();
                        b_isInfiniteFlipped = true;
                    }
                    else if (!b_InfiniteAmmo && b_isInfiniteFlipped && isInfiniteFlip > 0) // if infintieAmmo more than 0 & is increased by our hack.
                    {
                        localPlayerController.ammo.infiniteAmmo.UnFlip();
                        b_isInfiniteFlipped = false;
                    }
                    
                    // Freeze Max Ammo.
                    if (b_freezeMaxAmmo)
                    {
                        localPlayerController.gun.gunData.maxAmmo = 1;
                    }
                    else if (!b_freezeMaxAmmo)
                        localPlayerController.gun.gunData.maxAmmo = copy_GunData.maxAmmo;
                   
                    // Instant Shoot.
                    if (b_FireRate)
                    {
                        localPlayerController.gun.gunData.shotCooldown = 0.01f;
                    }
                    else if (!b_FireRate)
                        localPlayerController.gun.gunData.shotCooldown = copy_GunData.shotCooldown;
                    
                    //ReRoll.
                    if (b_ReRoll)
                        if (localPlayerController.loadedCharacter.nameStringID.key != "shana_name")
                            PowerupGenerator.CanReroll = true;
                    else if (!b_ReRoll)
                        if (localPlayerController.loadedCharacter.nameStringID.key != "shana_name")
                            PowerupGenerator.CanReroll = false;
                }
            }
            else if (!b_isPlaying)
            {
                if (b_isInit)
                {
                    localPlayerController.gun.gunData.reloadDuration = copy_GunData.reloadDuration;
                    localPlayerController.gun.gunData.maxAmmo = copy_GunData.maxAmmo;
                    localPlayerController.gun.gunData.shotCooldown = copy_GunData.shotCooldown;
                }
                b_isInit = false;
            }
        }
    }
}
