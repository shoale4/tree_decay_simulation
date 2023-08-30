using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TreeGenerator
{
    public class Tree
    {
        bool DoneGrowing = false;
        Random Random;

        Vector2 Position = Vector2.Zero;

        int LeafCount = 400;
        int TreeWidth = 80;    
        int TreeHeight = 150;   
        int TrunkHeight = 40;
        int MinDistance = 2;
        int MaxDistance = 15;
        int BranchLength = 2;

        Branch Root;
        List<Leaf> Leaves;
        Dictionary<Vector2, Branch> Branches;

        Rectangle Crown;
        Texture2D BlankTexture;

        SpriteFont Font;

        public Tree(Vector2 position, GraphicsDevice gd, ContentManager content)
        {
            Random = new Random();
            Position = position;

            LeafCount = Random.Next(400, 600);
            TreeWidth = Random.Next(50, 200);
            TreeHeight = Random.Next(100, 150);
            TrunkHeight = Random.Next(30, 60);
            MinDistance = Random.Next(2, 4);
            MaxDistance = Random.Next(15, 30);
            BranchLength = Random.Next(2, 6);

            LoadContent(gd, content);
            GenerateCrown();
            GenerateTrunk();
        }

        private void LoadContent(GraphicsDevice gd, ContentManager content)
        {
            int width = 10, height = 10;
            BlankTexture = new Texture2D(gd, 10, 10, false, SurfaceFormat.Color);
            Color[] color = new Color[width * height];
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = Color.White;
            }
            BlankTexture.SetData(color);
            
            Font = content.Load<SpriteFont>(@"font");
        }

        private void GenerateCrown()
        {
            Crown = new Rectangle((int)Position.X - TreeWidth / 2, (int)Position.Y - TreeHeight - TrunkHeight, TreeWidth, TreeHeight);
            Leaves = new List<Leaf>();
            
            //randomly place leaves within our rectangle
            for (int i = 0; i < LeafCount; i++)
            {
                Vector2 location = new Vector2(Random.Next(Crown.Left, Crown.Right + 1), Random.Next(Crown.Top, Crown.Bottom + 1));
                Leaf leaf = new Leaf(location, BlankTexture);
                Leaves.Add(leaf);
            }
        }
        
        private void GenerateTrunk()
        {
            //Branches = new HashSet<Branch>();
            Branches = new Dictionary<Vector2, Branch>();

            Root = new Branch(null, Position, new Vector2(0, -1), BlankTexture);
            Branches.Add(Root.Position, Root);

            Branch current = new Branch(Root, new Vector2(Position.X, Position.Y - BranchLength), new Vector2(0, -1), BlankTexture);
            Branches.Add(current.Position, current);

            //Keep growing trunk upwards until we reach a leaf       
            while ((Root.Position - current.Position).Length() < TrunkHeight)
            {
                Branch trunk = new Branch(current, new Vector2(current.Position.X, current.Position.Y - BranchLength), new Vector2(0, -1), BlankTexture);
                Branches.Add(trunk.Position, trunk);
                current = trunk;                
            }         
        }

        public void Grow()
        {
            if (DoneGrowing) return;

            //If no leaves left, we are done
            if (Leaves.Count == 0) { DoneGrowing = true; return; }

            //process the leaves
            for (int i = 0; i < Leaves.Count; i++)
            {
                bool leafRemoved = false;

                Leaves[i].ClosestBranch = null;
                Vector2 direction = Vector2.Zero;

                //Find the nearest branch for this leaf
                foreach (Branch b in Branches.Values)
                {
                    direction = Leaves[i].Position - b.Position;                       //direction to branch from leaf
                    float distance = (float)Math.Round(direction.Length());            //distance to branch from leaf
                    direction.Normalize();

                    if (distance <= MinDistance)            //Min leaf distance reached, we remove it
                    {
                        Leaves.Remove(Leaves[i]);                        
                        i--;
                        leafRemoved = true;
                        break;
                    }
                    else if (distance <= MaxDistance)       //branch in range, determine if it is the nearest
                    {
                        if (Leaves[i].ClosestBranch == null)
                            Leaves[i].ClosestBranch = b;
                        else if ((Leaves[i].Position - Leaves[i].ClosestBranch.Position).Length() > distance)
                            Leaves[i].ClosestBranch = b;
                    }
                }

                if (!leafRemoved)
                {
                    //Set the grow parameters on all the closest branches that are in range
                    if (Leaves[i].ClosestBranch != null)
                    {
                        Vector2 dir = Leaves[i].Position - Leaves[i].ClosestBranch.Position;
                        dir.Normalize();
                        Leaves[i].ClosestBranch.GrowDirection += dir;       //add to grow direction of branch
                        Leaves[i].ClosestBranch.GrowCount++;
                    }
                }
            }

            //Generate the new branches
            HashSet<Branch> newBranches = new HashSet<Branch>();
            foreach (Branch b in Branches.Values)
            {
                if (b.GrowCount > 0)    //if at least one leaf is affecting the branch
                {
                    Vector2 avgDirection = b.GrowDirection / b.GrowCount;
                    avgDirection.Normalize();

                    Branch newBranch = new Branch(b, b.Position + avgDirection * BranchLength, avgDirection, BlankTexture);

                    newBranches.Add(newBranch);
                    b.Reset();
                }
            }

            if (newBranches.Count == 0) { DoneGrowing = true; return; }

            //Add the new branches to the tree
            bool BranchAdded = false;
            foreach (Branch b in newBranches)
            {
                //Check if branch already exists.  These cases seem to happen when leaf is in specific areas
                Branch existing;
                if (!Branches.TryGetValue(b.Position, out existing))
                {
                    Branches.Add(b.Position, b);
                    BranchAdded = true;

                    //increment the size of the older branches, direct path to root
                    b.Size = 0.002f;
                    Branch p = b.Parent;
                    while (p != null)
                    {
                        if (p.Parent != null)
                            p.Parent.Size = p.Size + 0.001f;

                        p = p.Parent;

                    }
                }
            }

            if (!BranchAdded) 
                DoneGrowing = true;
        }

        public void Draw(SpriteBatch spritebatch, Camera camera)
        {
            spritebatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, camera.View);

            foreach (Leaf l in Leaves)
                l.Draw(spritebatch);

            foreach (Branch b in Branches.Values)
                b.Draw(spritebatch);

            spritebatch.End();
            
            //Draw Information
            spritebatch.Begin();
            spritebatch.DrawString(Font, "CONTROLS:  P = New Random Tree,  Spacebar = Grow Tree,  MouseWheel = Zoom,  J/I/K/L = Move Camera", new Vector2(0, 0), Color.Black);

            spritebatch.DrawString(Font,"Total Branches: " + Branches.Count.ToString(), new Vector2(0,28), Color.White);
            spritebatch.DrawString(Font, "Total Leaves: " + Leaves.Count.ToString(), new Vector2(0, 42), Color.White);
            spritebatch.DrawString(Font, "Crown Width: " + TreeWidth.ToString(), new Vector2(0, 56), Color.White);
            spritebatch.DrawString(Font, "Crown Height: " + TreeHeight.ToString(), new Vector2(0, 70), Color.White);
            spritebatch.DrawString(Font, "Trunk Height: " + TrunkHeight.ToString(), new Vector2(0, 84), Color.White);
            spritebatch.DrawString(Font, "Min. Leaf Distance: " + MinDistance.ToString(), new Vector2(0, 98), Color.White);
            spritebatch.DrawString(Font, "Max. Leaf Distance: " + MaxDistance.ToString(), new Vector2(0, 112), Color.White);
            spritebatch.DrawString(Font, "Branch Length: " + BranchLength.ToString(), new Vector2(0, 126), Color.White);

            if (!DoneGrowing)
                spritebatch.DrawString(Font, "Status: " + "Growing", new Vector2(0, 154), Color.White);
            else
                spritebatch.DrawString(Font, "Status: " + "Done", new Vector2(0, 154), Color.White);

            spritebatch.End();
        }
    }
}
