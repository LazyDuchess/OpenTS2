using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.SimAntics
{
    public class SimAnticsException : Exception
    {
        public VMStackFrame StackFrame;
        public SimAnticsException(string message, VMStackFrame stackFrame) : base(message)
        {
            StackFrame = stackFrame;
        }

        public override string ToString()
        {
            var trace = "";
            if (StackFrame != null)
            {
                trace += Environment.NewLine;
                trace += "Object: " + StackFrame.Thread.Entity.ObjectDefinition.FileName+" ("+StackFrame.Thread.Entity.ID+")";
                trace += Environment.NewLine;
                trace += "Stack Trace:";
                trace += Environment.NewLine;
                foreach(var elem in StackFrame.Thread.Frames)
                {
                    trace += "Frame" + Environment.NewLine;
                    trace += "Stack Object ID: " + elem.StackObjectID + Environment.NewLine;
                    trace += "Node: " + elem.CurrentNode + Environment.NewLine;
                    trace += "Tree: " + elem.BHAV.FileName + " ("+elem.BHAV.GlobalTGI.InstanceID+ ")" + Environment.NewLine;
                }
            }
            return Message + trace;
        }
    }
}
