using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class BlockEditor : MonoBehaviour
{
    public float EditDistance;
    public MenuManager Menu;

    private void Update()
    {
        // Place Block
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, EditDistance))
            {
                if (Menu.SlotData[Menu.SlotSelected].Blockcount > 0)
                {
                    // find the block position we need to edit
                    Vector3 PointInBlock = hit.point - transform.forward * .01f - hit.collider.gameObject.transform.position;

                    // round the position to an int for a refrence to the voxels array in the chunk
                    int x = Mathf.RoundToInt(PointInBlock.x);
                    int y = Mathf.RoundToInt(PointInBlock.y);
                    int z = Mathf.RoundToInt(PointInBlock.z);

                    // find blocktype we need to place
                    int indextoadd = Menu.SlotData[Menu.SlotSelected].BlockIndex;
                    // remove one block count
                    Menu.SlotData[Menu.SlotSelected].Blockcount -= 1;
                    // set the block text to the amount
                    Menu.BlockCounts[Menu.SlotSelected].text = Menu.SlotData[Menu.SlotSelected].Blockcount.ToString();

                    // check if the slot currently selected has no items in it. If so clear the slot and leave it.
                    if (Menu.SlotData[Menu.SlotSelected].Blockcount == 0)
                    {
                        // set the slot to be transparant
                        Menu.Slots[Menu.SlotSelected].color = new Color(0, 0, 0, 0);
                        // remove the text
                        Menu.SlotData[Menu.SlotSelected].tex = null;
                        // set the slots texture to null
                        Menu.Slots[Menu.SlotSelected].texture = null;
                    }

                    // set the block the the slot block
                    hit.collider.gameObject.GetComponent<MainVoxel>().UpdateChunk(new int3[] { new int3(x, y, z) }, new int[] { indextoadd });
                }
            }
        }

        // Remove Block
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, EditDistance))
            {
                // find the block position we need to edit
                Vector3 PointInBlock = hit.point + transform.forward * .01f - hit.collider.gameObject.transform.position;

                // round the position to an int for a refrence to the voxels array in the chunk
                int x = Mathf.RoundToInt(PointInBlock.x);
                int y = Mathf.RoundToInt(PointInBlock.y);
                int z = Mathf.RoundToInt(PointInBlock.z);

                // find the type of block you removed
                int blockindex = hit.collider.gameObject.GetComponent<MainVoxel>().Voxels[convert(x, y, z)];

                // go through the slots and ...
                for (int i = 0; i < 8; i++)
                {
                  // check if the slot you are on is empty...
                    if (Menu.SlotData[i].tex == null || Menu.SlotData[i].BlockIndex == blockindex)
                    {
                        // if the slot is empty then add one to the block count,
                        Menu.SlotData[i].Blockcount += 1;
                        // set the slots block index to the broken block
                        Menu.SlotData[i].BlockIndex = blockindex;
                        // set the texture of the slot to the one from the block that was broken
                        Menu.SlotData[i].tex = Menu.Blocks[Menu.GetBlockFromIndex(blockindex)].tex;
                        // set the slot to not be transparant
                        Menu.Slots[i].color = new Color(1, 1, 1, 1);
                        // set the texture of the slot to the one from the block that was broken
                        Menu.Slots[i].texture = Menu.SlotData[i].tex;
                        // set the block count text to be the amount of those blocks you have
                        Menu.BlockCounts[i].text = Menu.SlotData[i].Blockcount.ToString();
                        // end the loop
                        break;
                    }
                }


                hit.collider.gameObject.GetComponent<MainVoxel>().UpdateChunk(new int3[] { new int3(x, y, z) }, new int[] { 0 });
            }
        }
    }

    int convert(int x, int y, int z) => x + 16 * (y + 90 * z);

}
