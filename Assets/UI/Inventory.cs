using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxInventorySize = 20;

    [Header("Starting Weapons (for testing)")]
    [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();

    private List<WeaponData> weapons = new List<WeaponData>();

    private PlayerEquipment playerEquipment;

    void Start()
    {
        playerEquipment = GetComponent<PlayerEquipment>();

        // Add starting weapons for testing
        foreach (WeaponData weapon in startingWeapons)
        {
            if (weapon != null)
            {
                AddWeapon(weapon);
            }
        }
    }

    // Add a weapon to inventory
    public bool AddWeapon(WeaponData weapon)
    {
        if (weapon == null) return false;

        // Check if inventory is full
        if (weapons.Count >= maxInventorySize)
        {
            Debug.Log("Inventory full! Can't pick up " + weapon.weaponName);
            return false;
        }

        weapons.Add(weapon);
        Debug.Log("Picked up: " + weapon.weaponName);
        return true;
    }

    // Remove a weapon from inventory
    public void RemoveWeapon(WeaponData weapon)
    {
        if (weapons.Contains(weapon))
        {
            weapons.Remove(weapon);
            Debug.Log("Removed from inventory: " + weapon.weaponName);
        }
    }

    // Equip a weapon from inventory
    public void EquipWeaponFromInventory(WeaponData weapon)
    {
        if (!weapons.Contains(weapon))
        {
            Debug.Log("Weapon not in inventory!");
            return;
        }

        if (playerEquipment == null) return;

        // Determine where to equip based on weapon type
        if (weapon.weaponType == WeaponType.TwoHanded)
        {
            // Two-handed weapons go in main hand
            playerEquipment.EquipMainHand(weapon);
            Debug.Log("Equipped " + weapon.weaponName + " (Two-Handed)");
        }
        else if (weapon.weaponType == WeaponType.OneHanded)
        {
            // One-handed weapons - equip in main hand by default
            // Player can manually move to offhand later if desired
            playerEquipment.EquipMainHand(weapon);
            Debug.Log("Equipped " + weapon.weaponName + " (Main Hand)");
        }
        else if (weapon.weaponType == WeaponType.Shield)
        {
            // Shields always go in offhand
            playerEquipment.EquipOffHand(weapon);
            Debug.Log("Equipped " + weapon.weaponName + " (Off Hand)");
        }
    }

    // Get all weapons in inventory
    public List<WeaponData> GetWeapons()
    {
        return new List<WeaponData>(weapons); // Return a copy
    }

    // Get inventory size info
    public int GetWeaponCount()
    {
        return weapons.Count;
    }

    public int GetMaxInventorySize()
    {
        return maxInventorySize;
    }

    public bool IsFull()
    {
        return weapons.Count >= maxInventorySize;
    }
}