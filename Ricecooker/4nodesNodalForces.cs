﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper;
using mikity.NumericalMethodHelper.elements;
using mikity.NumericalMethodHelper.objects;
using mikity.NumericalMethodHelper.materials;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class four_nodes_nodal_forces : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.four-nodes-nodal-force.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public four_nodes_nodal_forces()
            : base("4nodes->nodalForces [Nodal Force]", "4nodes->nodalForces", "4nodes->nodalForces", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point3", "P3", "Third point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point4", "P4", "Fourth point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Number", "nV", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddVectorParameter("Force", "Force", "Force (Vector3d)", Grasshopper.Kernel.GH_ParamAccess.item, new Rhino.Geometry.Vector3d(0, 0, -1));
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);

        }

        private const int _nNodes = 4;
        private const int _dim = 2;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        List<Rhino.Geometry.Point3d> newNodes = new List<Rhino.Geometry.Point3d>();
        List<Rhino.Geometry.Line> lGeometry=new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> lGeometry2=new List<Rhino.Geometry.Line>();
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d(0, 0, -1);
        nodalForce nF;
        int nNewNodes;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!DA.GetData(6, ref v)) return;

            if (!FriedChiken.isInitialized)
            {

                GH_Point[] pointList = new GH_Point[8];
                List<GH_Point> tmpPointList = new List<GH_Point>();

                int[] nEdgeNodes = new int[_dim];
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref pointList[2]);
                DA.GetData(3, ref pointList[3]);
                DA.GetData(4, ref nEdgeNodes[0]);
                DA.GetData(5, ref nEdgeNodes[1]);
                for (int i = 0; i < _dim; i++)
                {
                    if (nEdgeNodes[i] < 2)
                    {
                        AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Integers must be greater than or equal to 2");
                        return;
                    }
                }
                //点群生成
                double[,] wt = mikity.MathUtil.bicubic(_dim, nEdgeNodes);
                nNewNodes = wt.GetLength(0);
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    particles[i] = new particle(0, 0, 0);
                    for (int j = 0; j < _nNodes; j++)
                    {
                        particles[i][0] += pointList[j].Value.X * wt[i, j];
                        particles[i][1] += pointList[j].Value.Y * wt[i, j];
                        particles[i][2] += pointList[j].Value.Z * wt[i, j];
                    }
                }
                pS = new GH_particleSystem(particles);
                node[] lNodes = new node[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    lNodes[i] = new node(i);
                    lNodes[i].copyFrom(pS.Value.particles);
                }
                nF = new nodalForce(v.X, v.Y, v.Z);
                for (int i = 0; i < nNewNodes; i++)
                {
                    nF.addNode(lNodes[i]);
                }
                pS.Value.addObject(nF);
                lGeometry.Clear();
                lGeometry2.Clear();
                for (int i = 0; i < nNewNodes; i++)
                {
                    lGeometry.Add(new Rhino.Geometry.Line(new Rhino.Geometry.Point3d(particles[i][0], particles[i][1], particles[i][2]), v));
                }
                this.DVPW = GetDVPW(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
            }
            else
            {
                nF.forceX = v.X;
                nF.forceY = v.Y;
                nF.forceZ = v.Z;
            }
            DA.SetData(0, pS);
        }
        public UpdateGeometry GetUPGR(List<Rhino.Geometry.Line> m)
        {
            return new UpdateGeometry((x, y, z) =>
                {
                    m.Clear();
                    for (int i = 0; i < nNewNodes; i++)
                    {
                        m.Add(new Rhino.Geometry.Line(new Rhino.Geometry.Point3d(pS.Value[i][0]+x, pS.Value[i][1]+y, pS.Value[i][2]+z), v));
                    }

                });
        }
        public DrawViewPortWire GetDVPW(List<Rhino.Geometry.Line> m)
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (this.Attributes.Selected)
                {
                    args.Display.DrawArrows(m, System.Drawing.Color.Red);
                }
                else
                {
                     args.Display.DrawArrows(m, System.Drawing.Color.DarkBlue);
                }

            });
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0bfbe58e-acd6-4aeb-be52-dbfce494d060"); }
        }

    }
}
