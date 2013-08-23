using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper.objects;
namespace mikity.ghComponents
{
  /*  public class GH_Catenary : Grasshopper.Kernel.GH_Component
    {
        //protected override System.Drawing.Bitmap Icon
        //{
        //    get
        //    {
        //        //現在のコードを実行しているAssemblyを取得
        //        System.Reflection.Assembly myAssembly =
        //            System.Reflection.Assembly.GetExecutingAssembly();
    //
     //           System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon29.bmp");
      //          //指定されたマニフェストリソースを読み込む
       //         System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
       //         return bmp;
       //     }
       // }
        

        public GH_Catenary()
            : base("Catenary", "Catenary", "Catenary", "Bath", "Catenary")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("End 1", "P1", "The first node", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("End 2", "P2", "The first node", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("numLink", "N", "The number of links", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GeometryParam("Geometry", "Geom", "Geometry", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_GenericParam("Force", "Force", "Force", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_GenericParam("Accel", "Accel", "Acceleration", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_GenericParam("Velocity", "Veloc", "Velocity", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_GenericParam("Coord", "Coord", "Position", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_GenericParam("Jacobian", "Jacob", "Jacobian of constraint conditions", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_GenericParam("Residual", "Residual", "Residual of constraint conditions", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            System.Windows.Forms.ToolStripMenuItem __m = Menu_AppendItem(menu, "Go?", Menu_MyCustomItemClicked);

            if (_go == true)
            {
                __m.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m.CheckState = System.Windows.Forms.CheckState.Unchecked;
                t = 0;
            }
        }
        private bool _go = false;
        private void Menu_MyCustomItemClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem __m = sender as System.Windows.Forms.ToolStripMenuItem;
            if (__m.CheckState == System.Windows.Forms.CheckState.Checked)
            {
                __m.CheckState = System.Windows.Forms.CheckState.Unchecked;
                _go = false;
                t = 0;
            }
            else if (__m.CheckState == System.Windows.Forms.CheckState.Unchecked)
            {
                __m.CheckState = System.Windows.Forms.CheckState.Checked;
                _go = true;
            }
            this.ComputeData();
            this.m_attributes.PerformLayout();
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            _go = false;//default value;
            reader.TryGetBoolean("Go?", ref _go);
            return base.Read(reader);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("Go?", false);
            return base.Write(writer);
        }

        static int t = 0;
        List<Rhino.Geometry.Line> lL = new List<Rhino.Geometry.Line>();
        Rhino.Geometry.Point3d E1 = new Rhino.Geometry.Point3d(), E2=new Rhino.Geometry.Point3d();
        GH_vector lW = null;  //Force
        GH_vector lA = null;  //Acceleration
        GH_vector lV = null;  //Velocity
        GH_vector lX = null;  //Coordinate
        GH_vector lR = null;  //Residual of constraint conditions
        GH_matrix mJ = null;  //Jacobian matrix
        int numLink = 0;
        int numNode = 0;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (_go == false)
            {
                t = 0;
            }else
            {
                if (t == 0)
                {
                    if (!DA.GetData(0, ref E1)) return;
                    if (!DA.GetData(1, ref E2)) return;
                    if (!DA.GetData(2, ref numLink)) return;
                    numNode = numLink + 1;
                    lW = new GH_vector();
                    lA = new GH_vector();
                    lV = new GH_vector();
                    lX = new GH_vector();
                    lR = new GH_vector();
                    mJ = new GH_matrix();
                    lW.Value = new vector(numNode*3).zeros();
                    lA.Value = new vector(numNode * 3).zeros();
                    lV.Value = new vector(numNode * 3).zeros();
                    lX.Value = new vector(numNode * 3).zeros();
                    lR.Value = new vector(numLink).zeros();
                    mJ.Value = new matrix(numLink, numNode * 3).zeros();
                    //Initial configuration
                    lX.Value[0] = E1.X;
                    lX.Value[1] = E1.Y;
                    lX.Value[2] = E1.Z;
                    double dx, dy, dz;
                    dx = (E2.X - E1.X) / numLink;
                    dy = (E2.Y - E1.Y) / numLink;
                    dz = (E2.Z - E1.Z) / numLink;
                    for (int i = 1; i < numNode; i++)
                    {
                        lX.Value[i * 3 + 0] = lX.Value[(i - 1) * 3 + 0] + dx;
                        lX.Value[i * 3 + 1] = lX.Value[(i - 1) * 3 + 1] + dy;
                        lX.Value[i * 3 + 2] = lX.Value[(i - 1) * 3 + 2] + dz;
                    }
                }
                DA.SetDataList(0, lL); //Geomtry
                DA.SetData(1, lW);//Omega
                DA.SetData(2, lA);//r
                DA.SetData(3, lV);//q
                DA.SetData(4, lX);//x
                DA.SetData(5, mJ);//Jacobian
                DA.SetData(6, lR);//residual
                t++;
            }

            return;
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("47e406f9-cc06-469c-8d3a-2d3d13133837"); }
        }

        private void computeJacobian()
        {
            mJ.zeros();

            for (int Link = 0; Link < numLink; Link++)
            {
                double px = lX.Value[Link * 3 + 0];
                double py = lX.Value[Link * 3 + 1];
                double pz = lX.Value[Link * 3 + 2];
                double qx = lX.Value[(Link + 1) * 3 + 0];
                double qy = lX.Value[(Link + 1) * 3 + 1];
                double qz = lX.Value[(Link + 1) * 3 + 2];
                double dx = px - qx;
                double dy = py - qy;
                double dz = pz - qz;
                double Length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                mJ.Value[Link, Link * 3 + 0] = (px - qx) / Length;
                mJ.Value[Link, Link * 3 + 1] = (py - qy) / Length;
                mJ.Value[Link, Link * 3 + 2] = (pz - qz) / Length;
            }
            for (int i = 0; i < numLink; i++)
            {
                mJ.Value[i, 0] = 0;
                mJ.Value[i, 1] = 0;
                mJ.Value[i, 2] = 0;
                mJ.Value[i, (numNode - 1) * 3] = 0;
                mJ.Value[i, (numNode - 1) * 3 + 1] = 0;
                mJ.Value[i, (numNode - 1) * 3 + 2] = 0;
            }
        }
        void computeResidual()
        {
            for (int Link = 0; Link < numLink; Link++)
            {
                double px = Coord.get(Link * 2 + 0, 0);
                double py = Coord.get(Link * 2 + 1, 0);
                double qx = Coord.get((Link + 1) * 2 + 0, 0);
                double qy = Coord.get((Link + 1) * 2 + 1, 0);
                double dx = px - qx;
                double dy = py - qy;
                double Length = Math.sqrt(dx * dx + dy * dy);
                double r = Length - RequiredLength;
                Residual.set(Link, 0, r);
            }
        }
        void computeGradient()
        {
            for (int Node = 0; Node < numNode; Node++)
            {
                Grad.set(0, Node * 2 + 0, 0.0);
                Grad.set(0, Node * 2 + 1, 1.0 * gravity);
            }
            Grad.set(0, 0, 0);
            Grad.set(0, 1, 0);
            Grad.set(0, (numNode - 1) * 2, 0);
            Grad.set(0, (numNode - 1) * 2 + 1, 0);
        }

    }
*/
    unsafe public delegate res _function(double x,double y);
    public class function
    {
        public _function Func;

    }
    public struct res
    {
       public double z,zDiffX,zDiffY;
    }
    public class hill
    {
        public double x;
        public double y;
        public double size;
        public double h;
        public hill(double _x,double _y,double _size,double _h)
        {
            this.x=_x;
            this.y=_y;
            this.size=_size;
            this.h=_h;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GH_function : GH_Goo<function>
    {
        public double size;
        public int uMax, vMax;
        public double maxX, maxY;
        public double c;
        public double seaLevel;
        public double ContourInterval;
        public double a, bsin, bcos;
        public GH_function()
        {
        }
        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override bool IsValid
        {
            get
            {
                if (this.Value != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("");
        }

        public override string TypeDescription
        {
            get { return "Function to be evaluated"; }
        }

        public override string TypeName
        {
            get { return "function"; }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GH_hill : GH_Goo<hill>
    {
        public GH_hill(double _x,double _y,double _size,double _h)
        {
            this.Value = new hill(_x,_y,_size,_h);
        }
        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override bool IsValid
        {
            get
            {
                if (this.Value != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("center=[{0},{1}],height={2},size={3}",this.Value.x,this.Value.y,this.Value.h,this.Value.size);
        }

        public override string TypeDescription
        {
            get { return "Deifinition of each hill"; }
        }

        public override string TypeName
        {
            get { return "hill"; }
        }
    }

    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class bathHill : Grasshopper.Kernel.GH_Component
    {
/*        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon3.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        */
        public bathHill()
            : base("Hill", "Hill", "Hill", "Bath", "DoubleHexK")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("hillPositionX", "X", "X coordinate of the center of new hill",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("hillPositionY", "Y", "Y coordinate of the center of new hill",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("hillPlanSize", "Size", "Size of new hill",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("hillHight", "Height", "Height of new hill",Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("new Hill","Hill","Definition of new hill");
        }
             
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            double x = 0, y = 0, size = 0, h = 0;
            if (!DA.GetData(0, ref x)) return;
            if (!DA.GetData(1, ref y)) return;
            if (!DA.GetData(2, ref size)) return;
            if (!DA.GetData(3, ref h)) return;
            GH_hill newHill= new GH_hill(x, y, size, h);
            DA.SetData(0, newHill);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("73abcd20-025c-4ef8-accd-ca9b7561cb6b"); }
        }

    }
    public class bathFunction : Grasshopper.Kernel.GH_Component
    {
        /*        protected override System.Drawing.Bitmap Icon
                {
                    get
                    {
                        //現在のコードを実行しているAssemblyを取得
                        System.Reflection.Assembly myAssembly =
                            System.Reflection.Assembly.GetExecutingAssembly();

                        System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon3.bmp");
                        //指定されたマニフェストリソースを読み込む
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                        return bmp;
                    }
                }
                */
        public bathFunction()
            : base("Function", "Function", "Function", "Bath", "DoubleHexK")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Hills", "Hills", "List of Hills", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Size of Plan",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("uMax", "uMax", "Number of cells along u direction",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("vMax", "vMax", "Number of cells along v direction",Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "p", "points representing function to be evaluated", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddSurfaceParameter("SeaLevel", "Sea", "SeaLevel",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Function", "Func", "Function to be evaluated",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.HideParameter(0);
        }

        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            List<GH_hill> listHill = new List<GH_hill>();
            int uMax=0, vMax=0;
            double size = 0.0;
            if (!DA.GetDataList(0, listHill)) return;
            if (!DA.GetData(1, ref size)) return;
            if (!DA.GetData(2, ref uMax)) return;
            if (!DA.GetData(3, ref vMax)) return;
            double ContourInterval = 0.01 * listHill[0].Value.h;
            double seaLevel = 30.0 * ContourInterval;

            double c = size / uMax;
            double otherc = size / vMax;
            if (c > otherc) c = otherc;
            double maxX = c * uMax / 2d;
            double maxY = c * vMax / 2d;
            
            GH_function newFunction=new GH_function();
            newFunction.Value=new function();
            newFunction.Value.Func = new _function((xValue, yValue) => {
                res f=new res();
                double z = -seaLevel;
                double zDiff_X = 0;
                double zDiff_Y = 0;
                double dx = 0;
                double dy = 0;
                for (int hill = 0; hill <listHill.Count; hill++)
                {
                    dx = xValue - listHill[hill].Value.x;
                    dy = yValue - listHill[hill].Value.y;
                    double expThingy = listHill[hill].Value.h * Math.Exp(-(dx * dx + dy * dy) / (listHill[hill].Value.size* listHill[hill].Value.size));
                    z += expThingy;
                    zDiff_X -= dx * expThingy / (listHill[hill].Value.size * listHill[hill].Value.size);
                    zDiff_Y -= dy * expThingy / (listHill[hill].Value.size * listHill[hill].Value.size);
                }
                f.z=z;
                f.zDiffX=zDiff_X;
                f.zDiffY=zDiff_Y;

                return f;
            });
            Rhino.Geometry.PointCloud pc = new Rhino.Geometry.PointCloud();

            for (int i = 0; i <= uMax * 4; i++)
            {
                for (int j = 0; j <= vMax * 4; j++)
                {
                    double x = -maxX + (c/4d) * i;
                    double y = -maxY + (c/4d) * j;
                    pc.Add(new Rhino.Geometry.Point3d(x, y, newFunction.Value.Func(x, y).z));
                }
            }
            Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane(new Rhino.Geometry.Point3d(0, 0, 0), new Rhino.Geometry.Vector3d(1.0, 0, 0), new Rhino.Geometry.Vector3d(0.0, 1.0, 0));
            Rhino.Geometry.PlaneSurface planeSurf = new Rhino.Geometry.PlaneSurface(plane, new Rhino.Geometry.Interval(-size / 2d * 1.2d, size / 2d * 1.2d), new Rhino.Geometry.Interval(-size / 2d * 1.2d, size / 2d * 1.2d));
            DA.SetDataList(0, pc.GetPoints().ToList());
            DA.SetData(1, planeSurf);
            newFunction.size = size;
            newFunction.uMax = uMax;
            newFunction.vMax = vMax;
            newFunction.maxX = maxX;
            newFunction.maxY = maxY;
            newFunction.c=c;
            newFunction.seaLevel = seaLevel;
            newFunction.ContourInterval = ContourInterval;
            newFunction.a = c * (Math.Sqrt(7.0) - 1.0) / 6.0;
            newFunction.bsin = c / 4.0;
            newFunction.bcos = c * (4.0 - Math.Sqrt(7.0)) / 12.0;
            DA.SetData(2, newFunction);
        
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("48cac65e-f81e-42e5-bbde-3a742a1226b5"); }
        }

    }
    public class BathMainLoop : Grasshopper.Kernel.GH_Component
    {
        /*        protected override System.Drawing.Bitmap Icon
                {
                    get
                    {
                        //現在のコードを実行しているAssemblyを取得
                        System.Reflection.Assembly myAssembly =
                            System.Reflection.Assembly.GetExecutingAssembly();

                        System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon3.bmp");
                        //指定されたマニフェストリソースを読み込む
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                        return bmp;
                    }
                }
                */
        private double[,] x;
        private double[,] velocity;
        private double[,] force;
        double[,] originalX;
        int[] nodeType;
        int[] numberOfMembersAtNode, maxNumberOfMembersAtNode, numberFarEndsBoundary;
        int[] thisEnd, thatEnd, MemberType;
        public int numNode, numMember;
        public BathMainLoop()
            : base("MainLoop", "MainLoop", "MainLoop", "Bath", "DoubleHexK")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Function", "Func", "Function to be evaluated",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddBooleanParameter("Stop/Go", "?/!", "Stop/Go",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("Amplitude aplied to random number", "R", "Amplitude applied to random number",Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "N", "Nodes", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddLineParameter("Structure", "Lines", "Structure",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.HideParameter(0);
        }

        private int t=0;
        private List<Rhino.Geometry.Point3d> lP;
        private List<Rhino.Geometry.Line> lL;
        private GH_function newFunction;
        private double rs = 0;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            bool toggle=false;
            if (!DA.GetData(1, ref toggle)) return;
            if (!DA.GetData(2, ref rs)) return;
            if (toggle == false)
            {
                newFunction= new GH_function();
                if (!DA.GetData(0, ref newFunction)) return;
                t = 0;
                numNode = 12 * (newFunction.uMax) * (newFunction.vMax);
                int maxNumMember = 20 * (newFunction.uMax) * (newFunction.vMax);
                x = new double[numNode, 3];
                originalX = new double[numNode, 3];
                force = new double[numNode, 3];
                velocity = new double[numNode, 3];
                nodeType = new int[numNode];
                numberOfMembersAtNode = new int[numNode];
                maxNumberOfMembersAtNode = new int[numNode];
                numberFarEndsBoundary = new int[numNode];
                thisEnd = new int[maxNumMember];
                thatEnd = new int[maxNumMember];
                MemberType = new int[maxNumMember];

                lP = initNodes(newFunction, numNode);
                lL = initMembers(newFunction, numNode);
            }
            else
            {
                calculation();
                visualization();
                t++;
            }
            DA.SetDataList(0, lP);
            DA.SetDataList(1, lL);

        }
        private void visualization()
        {
            lL.Clear();
            for (int member = 0; member < numMember; member++)
            {
                if (MemberType[member] != 0) lL.Add(new Rhino.Geometry.Line(new Rhino.Geometry.Point3d(x[thisEnd[member], 0], x[thisEnd[member], 1], x[thisEnd[member], 2]), new Rhino.Geometry.Point3d(x[thatEnd[member], 0], x[thatEnd[member], 1], x[thatEnd[member], 2])));
            }
            lP.Clear();
            for (int node = 0; node < numNode; node++)
            {
                for (int xyz = 0; xyz <= 2; xyz++)
                {
                    lP.Add(new Rhino.Geometry.Point3d(x[node, 0], x[node, 1], x[node, 2]));
                }

            }

        }
        private void calculation()
        {
            for (int go = 0; go <1; go++)
            {
                for (int node = 0; node < numNode; node++)
                {
                    if (nodeType[node] != 0)
                    {
                        for (int xyz = 0; xyz <= 2; xyz++) force[node, xyz] = 0.0;
                    }
                }
                for (int member = 0; member < numMember; member++)
                {
                    if (MemberType[member] == 1)
                    {
                        double dx = x[thatEnd[member], 0] - x[thisEnd[member], 0];
                        double dy = x[thatEnd[member], 1] - x[thisEnd[member], 1];
                        double dz = x[thatEnd[member], 2] - x[thisEnd[member], 2];
                        for (int xyz = 0; xyz <= 2; xyz++)
                        {
                            force[thisEnd[member], 0] += dx;
                            force[thisEnd[member], 1] += dy;
                            force[thisEnd[member], 2] += dz;
                            force[thatEnd[member], 0] -= dx;
                            force[thatEnd[member], 1] -= dy;
                            force[thatEnd[member], 2] -= dz;
                        }
                    }
                }
                for (int node = 0; node < numNode; node++)
                {

                    res f = newFunction.Value.Func(x[node, 0], x[node, 1]);
                    if (nodeType[node] == 2)
                    {
                        double scalarProduct = 0.0;
                        double zDiffMagSq = 0.0;
                        scalarProduct += force[node, 0] * f.zDiffX;
                        scalarProduct += force[node, 1] * f.zDiffY;
                        zDiffMagSq += f.zDiffX * f.zDiffX + f.zDiffY * f.zDiffY;
                        force[node, 0] -= f.zDiffX * scalarProduct / zDiffMagSq;// This removes component of force perpendicular to the boundary
                        force[node, 1] -= f.zDiffY * scalarProduct / zDiffMagSq;// This removes component of force perpendicular to the boundary
                        force[node, 0] -= f.z * f.zDiffX / zDiffMagSq;//This moves node to boundary
                        force[node, 1] -= f.z * f.zDiffY / zDiffMagSq;//This moves node to boundary
                    }
                    force[node, 2] = 0.0;
                    x[node, 2] = 1.0 * f.z;
                }
                for (int node = 0; node < numNode; node++)
                {
                    if (nodeType[node] != 0)
                    {
                        for (int xy = 0; xy <= 1; xy++)
                        {
                            velocity[node, xy] = 0.98 * velocity[node, xy] + force[node, xy] / 50.0;
                            x[node, xy] += velocity[node, xy];
                        }
                    }
                }
            }
        }
        

        
        private List<Rhino.Geometry.Line> initMembers(GH_function func,int numNode)
        {
            List<Rhino.Geometry.Line> lL = new List<Rhino.Geometry.Line>();
            numMember = 0;
            for (int node = 0; node < numNode - 1; node++)
            {
                for (int otherNode = node + 1; otherNode <numNode; otherNode++)
                {
                    double dx = x[node, 0] - x[otherNode, 0];
                    double dy = x[node, 1] - x[otherNode, 1];
                    double LengthSq = dx * dx + dy * dy;
                    if (LengthSq < 1.1 * func.a * func.a)
                    {
                        MemberType[numMember] = 0;
                        if (nodeType[node] != 0 || nodeType[otherNode] != 0) MemberType[numMember] = 2;
                        if (nodeType[node] != 0 && nodeType[otherNode] != 0) MemberType[numMember] = 1;
                        thisEnd[numMember] = node;
                        thatEnd[numMember] = otherNode;
                        numMember++;
                    }
                }
            }
            for (int member = 0; member < numMember; member++)
            {
                maxNumberOfMembersAtNode[thisEnd[member]]++;
                maxNumberOfMembersAtNode[thatEnd[member]]++;
                if (MemberType[member] != 0)
                {
                    numberOfMembersAtNode[thisEnd[member]]++;
                    numberOfMembersAtNode[thatEnd[member]]++;
                }
            }
            //Bug???
            for (int node = 0; node <numNode - 1; node++)
            {
                if (numberOfMembersAtNode[node] > 1 && nodeType[node] == 0) nodeType[node] = 2;
                if (numberOfMembersAtNode[node] == 1 && nodeType[node] == 0) nodeType[node] = 3;
                numberOfMembersAtNode[node] = 0;
            }
            for (int member = 0; member < numMember; member++)
            {
                if (MemberType[member] == 0)
                {
                    if (nodeType[thisEnd[member]] != 0 && nodeType[thatEnd[member]] != 0) MemberType[member] = 2;
                }
                if (MemberType[member] != 0)
                {
                    numberOfMembersAtNode[thisEnd[member]]++;
                    numberOfMembersAtNode[thatEnd[member]]++;
                }
            }


            ////
            for (int node = 0; node <numNode - 1; node++)
            {
                if (numberOfMembersAtNode[node] <= 1) nodeType[node] = 0;
            }
            for (int member = 0; member < numMember; member++)
            {
                if (nodeType[thisEnd[member]] == 0 || nodeType[thatEnd[member]] == 0) MemberType[member] = 0;
            }

            for (int node = 0; node <numNode - 1; node++) numberOfMembersAtNode[node] = 0;

            for (int member = 0; member < numMember; member++)
            {
                if (MemberType[member] != 0)
                {
                    numberOfMembersAtNode[thisEnd[member]]++;
                    numberOfMembersAtNode[thatEnd[member]]++;
                }
            }

            for (int node = 0; node <numNode - 1; node++)
            {
                if (numberOfMembersAtNode[node] <= 1) nodeType[node] = 0;
                if (nodeType[node] == 1 && numberOfMembersAtNode[node] < maxNumberOfMembersAtNode[node]) nodeType[node] = 3;
            }

            for (int member = 0; member <numMember; member++)
            {
                if (MemberType[member] != 0)
                {
                    if (nodeType[thisEnd[member]] > 1) numberFarEndsBoundary[thatEnd[member]]++;
                    if (nodeType[thatEnd[member]] > 1) numberFarEndsBoundary[thisEnd[member]]++;
                }
            }

            for (int node = 0; node <numNode - 1; node++)
            {
                if (numberFarEndsBoundary[node] > 1 && nodeType[node] == 1) nodeType[node] = 2;
            }

            for (int node = 0; node <numNode - 1; node++)
            {
                if (nodeType[node] == 3) nodeType[node] = 2;
            }

            for (int member = 0; member <numMember; member++)
            {
                if (MemberType[member] == 2) MemberType[member] = 1;
            }

            for (int node = 0; node < numNode - 1; node++) numberFarEndsBoundary[node] = 0;

            for (int member = 0; member <numMember; member++)
            {
                if (MemberType[member] == 0)
                {
                    if (nodeType[thisEnd[member]] == 2) numberFarEndsBoundary[thatEnd[member]]++;
                    if (nodeType[thatEnd[member]] == 2) numberFarEndsBoundary[thisEnd[member]]++;
                }
            }

            for (int node = 0; node < numNode - 1; node++)
            {
                if (nodeType[node] == 0 && numberFarEndsBoundary[node] == 2) nodeType[node] = 2;
            }

            for (int member = 0; member < numMember; member++)
            {
                if (MemberType[member] == 0 && nodeType[thisEnd[member]] == 2 && nodeType[thatEnd[member]] == 2) MemberType[member] = 1;

                if (MemberType[member] != 0) lL.Add(new Rhino.Geometry.Line(new Rhino.Geometry.Point3d(x[thisEnd[member], 0], x[thisEnd[member], 1], x[thisEnd[member], 2]), new Rhino.Geometry.Point3d(x[thatEnd[member], 0], x[thatEnd[member], 1], x[thatEnd[member], 2])));
            }

            return lL;

        }
        private List<Rhino.Geometry.Point3d> initNodes(GH_function func,int numNode)
        {
            List<Rhino.Geometry.Point3d> lP = new List<Rhino.Geometry.Point3d>();
            {
                int node = -1;
                for (int u = 0; u < func.uMax; u++)
                {
                    for (int v = 0; v < func.vMax; v++)
                    {
                        for (int nodeInCell = 0; nodeInCell < 12; nodeInCell++)
                        {
                            node++;
                            double xBottomLeft = -func.maxX + (double)u * func.c;
                            double yBottomLeft = -func.maxY + (double)v * func.c;
                            if (nodeInCell == 0)
                            {
                                x[node, 0] = xBottomLeft + func.a / 2.0;
                                x[node, 1] = yBottomLeft;
                            }
                            if (nodeInCell == 1)
                            {
                                x[node, 0] = xBottomLeft + func.c - func.a / 2.0;
                                x[node, 1] = yBottomLeft;
                            }
                            if (nodeInCell == 2)
                            {
                                x[node, 0] = xBottomLeft + func.c / 2.0;
                                x[node, 1] = yBottomLeft + func.a / 2.0;
                            }
                            if (nodeInCell == 3)
                            {
                                x[node, 0] = xBottomLeft + func.a / 2.0 + func.bcos;
                                x[node, 1] = yBottomLeft + func.bsin;
                            }
                            if (nodeInCell == 4)
                            {
                                x[node, 0] = xBottomLeft + func.c - func.a / 2.0 - func.bcos;
                                x[node, 1] = yBottomLeft + func.bsin;
                            }
                            if (nodeInCell == 5)
                            {
                                x[node, 0] = xBottomLeft;
                                x[node, 1] = yBottomLeft + func.bsin + func.bcos;
                            }
                            if (nodeInCell == 6)
                            {
                                x[node, 0] = xBottomLeft + func.a / 2.0 + 2.0 * func.bcos;
                                x[node, 1] = yBottomLeft + func.c / 2.0;
                            }
                            if (nodeInCell == 7)
                            {
                                x[node, 0] = xBottomLeft + 3.0 * func.a / 2.0 + 2.0 * func.bcos;
                                x[node, 1] = yBottomLeft + func.c / 2.0;
                            }
                            if (nodeInCell == 8)
                            {
                                x[node, 0] = xBottomLeft;
                                x[node, 1] = yBottomLeft + func.bsin + func.bcos + func.a;
                            }
                            if (nodeInCell == 9)
                            {
                                x[node, 0] = xBottomLeft + func.bsin;
                                x[node, 1] = yBottomLeft + func.c - func.bsin;
                            }
                            if (nodeInCell == 10)
                            {
                                x[node, 0] = xBottomLeft + func.c - func.bsin;
                                x[node, 1] = yBottomLeft + func.c - func.bsin;
                            }
                            if (nodeInCell == 11)
                            {
                                x[node, 0] = xBottomLeft + func.c / 2.0;
                                x[node, 1] = yBottomLeft + func.c - func.a / 2.0;
                            }
                        }
                    }
                }
            if(numNode!=node+1)return lP;
            }
            System.Random rand = new System.Random(0);
            for (int node = 0; node < numNode; node++)
            {
                double z = func.Value.Func(x[node,0], x[node,1]).z;
                x[node, 0] = x[node, 0] + (rand.NextDouble() - 0.5) * rs;
                x[node, 1] = x[node, 1] + (rand.NextDouble() - 0.5) * rs;
                x[node, 2] = z;
                if (z < 0.0)
                {
                    nodeType[node] = 0;
                }
                else
                {
                    nodeType[node] = 1;
                }
                
                {
                    for (int xyz = 0; xyz <= 2; xyz++)
                    {
                        originalX[node, 0] = x[node, 0];
                        originalX[node, 1] = x[node, 1];
                        originalX[node, 2] = x[node, 2];
                        lP.Add(new Rhino.Geometry.Point3d(originalX[node,0],originalX[node,1],originalX[node,2]));
                        velocity[node, 0] = 0.0;
                        velocity[node, 1] = 0.0;
                        velocity[node, 2] = 0.0;
                    }
                    x[node, 2] = z;
                }
                numberOfMembersAtNode[node] = 0;
                numberFarEndsBoundary[node] = 0;
                maxNumberOfMembersAtNode[node] = 0;

            }
            return lP;
        }

        
        public override Guid ComponentGuid
        {
            get { return new Guid("e537f59f-4e12-4a50-b08a-6538a8bbd566"); }
        }

    }
}
