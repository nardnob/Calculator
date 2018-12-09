using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Calculator
{
	public partial class Main : Form
	{
		bool
			hasNum = false, //is there a number in the display
			endingOp = false, //was the last operation an "endingOp", as in an equals or reciprocal. No more typing digits until another op assigned
			memoryStored = false,
			displayedOp = false; //whether there is an op that is displaying.. +, -, *, /

		const int TO_NUM = 2; //index of the num in the substring of textBox_display .. i.e. "+ 2.0" would be 2 for the index of the '2' in "2.0"

		enum OP { NONE, ADD, SUB, DIV, MUL, EQU, RECIP, CLR, CLRENTRY, SQRT, INV, BACKSPACE, MSTORE, MRECALL, MADD, MCLEAR, PER };

		OP currentOperation = OP.NONE;

		double currentValue = 0, memoryValue = 0;

		string soonTxt = "0";

		public Main()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e) //load event
		{
			FormBorderStyle = FormBorderStyle.FixedSingle; //disable resizing
			MaximizeBox = false; //disable maximize box
			MinimizeBox = false; //disable minimize box

			button_equals.Focus();
		}

		private double getDigits()
		{
			double digits = 0;

			//Need to take in the digits, if DigitGrouping, then group them with commas 
			if(displayedOp)
			{
				if(soonTxt.Length > 3)
					digits = Double.Parse(soonTxt.Substring(TO_NUM, soonTxt.Length - 2));
				else
					if(soonTxt.Length == 3)
						digits = Double.Parse(soonTxt[2].ToString());

				//textBox_display.Text = soonTxt.Substring(0, TO_NUM - 1) + digits.ToString("N", CultureInfo.InvariantCulture);
			}
			else
			{
				if(soonTxt.Length > 0)
					digits = Double.Parse(soonTxt);
			}

			return digits;
		}

		private void displayText()
		{
			double digits = 0;
			int unincludeOp = 0;


			if(dgtGrouping.Checked && ((displayedOp && soonTxt.Length > 2) || (!displayedOp && soonTxt.Length > 0)))
			{
				digits = getDigits();

				//Need to take in the digits, if DigitGrouping, then group them with commas 

				if(!displayedOp)
					unincludeOp = TO_NUM;

				//check if has decimal
				if(digits % 1 == 0 && !soonTxt.Contains("."))
				{
					textBox_display.Text = soonTxt.Substring(0, TO_NUM - unincludeOp) + digits.ToString("N0");
				}
				else 
				{
					textBox_display.Text = soonTxt.Substring(0, TO_NUM - unincludeOp) + digits.ToString("#,##0.##########"); //("g");
				}

				if(soonTxt[soonTxt.Length - 1] == '.')
					textBox_display.Text += ".";
			}
			else
				textBox_display.Text = soonTxt;
		}

		private void appendToDisplay(String in_text)
		{
			if(soonTxt == "0") //if the text is just 0, take it out to replace it
			{
				if(in_text == ".")
				{
					//textBox_display.Text = in_text;
					soonTxt = "0.";
				}
				else
				{
					//textBox_display.Text = in_text;
					soonTxt = in_text;
				}

				displayText();
				hasNum = true;
			}
			else
				if(!hasNum) //if the text is some operand plus nothing else, i.e. "* " to prevent pre-zeros "* 0003"
				{
					if(in_text != "0" && !endingOp)
					{
						if(in_text == ".")
							soonTxt += "0.";
						else
							soonTxt += in_text;

						displayText();
						hasNum = true;
					}
				}
				else
				{
					//textBox_display.Text += in_text;
					soonTxt += in_text;
					displayText();
					hasNum = true;
				}
		}

		private void equateDisplay()
		{
			switch(currentOperation)
			{
			case OP.NONE:
				if(!hasNum)
					currentValue = 0;
				else
					currentValue = double.Parse(soonTxt.Substring(0));
				break;
			case OP.ADD:
				if(hasNum)
					currentValue += double.Parse(soonTxt.Substring(TO_NUM));
				break;
			case OP.SUB:
				if(hasNum)
					currentValue -= double.Parse(soonTxt.Substring(TO_NUM));
				break;
			case OP.DIV:
				if(hasNum)
					currentValue /= double.Parse(soonTxt.Substring(TO_NUM));
				break;
			case OP.MUL:
				if(hasNum)
					currentValue *= double.Parse(soonTxt.Substring(TO_NUM));
				break;
			default:
				currentValue = double.Parse(soonTxt.Substring(0));
				break;
			}
		}

		private void setOperation(OP in_op)
		{
			if(in_op != OP.CLRENTRY
				&& in_op != OP.BACKSPACE
				&& in_op != OP.MRECALL
				&& in_op != OP.PER)
			{
				//solve the current display
				equateDisplay();
			}
			
			bool newOpIn = in_op == OP.ADD || in_op == OP.SUB || in_op == OP.MUL || in_op == OP.DIV;

			//set whether or not an op is displaying (like + - * or /).. useful for parsing string
			if( newOpIn || (displayedOp && (in_op == OP.MRECALL || in_op == OP.PER)) )
				displayedOp = true;
			else
				if(in_op != OP.BACKSPACE)
					displayedOp = false;

			//set the operation for the next display
			if(in_op != OP.BACKSPACE
				&& in_op != OP.MRECALL
				&& in_op != OP.PER)
				currentOperation = in_op;

			if(in_op != OP.BACKSPACE
				&& in_op != OP.MRECALL
				&& in_op != OP.PER)
				hasNum = false;

			if(in_op != OP.MRECALL)
				endingOp = false;

			//display the operation in the display
			switch(in_op)
			{
			case OP.ADD:
				//textBox_display.Text = "+ ";
				soonTxt = "+ ";
				displayText();
				break;
			case OP.SUB:
				//textBox_display.Text = "- ";
				soonTxt = "- ";
				displayText();
				break;
			case OP.DIV:
				//textBox_display.Text = "/ ";
				soonTxt = "/ ";
				displayText();
				break;
			case OP.MUL:
				//textBox_display.Text = "* ";
				soonTxt = "* ";
				displayText();
				break;
			case OP.EQU:
				endingOp = true;
				//textBox_display.Text = currentValue.ToString();
				soonTxt = currentValue.ToString();
				displayText();
				hasNum = false;
				break;
			case OP.RECIP:
				endingOp = true;
				currentValue = 1 / currentValue;
				//textBox_display.Text = currentValue.ToString();
				soonTxt = currentValue.ToString();
				displayText();
				hasNum = false;
				break;
			case OP.SQRT:
				endingOp = true;
				currentValue = Math.Sqrt(currentValue);
				//textBox_display.Text = currentValue.ToString();
				soonTxt = currentValue.ToString();
				displayText();
				hasNum = false;
				break;
			case OP.INV:
				endingOp = true;
				currentValue *= -1;
				//textBox_display.Text = currentValue.ToString();
				soonTxt = currentValue.ToString();
				displayText();
				hasNum = false;
				break;
			case OP.CLR:
				//textBox_display.Text = "0";
				soonTxt = "0";
				displayText();
				currentValue = 0;
				currentOperation = OP.NONE;
				//memoryValue = 0;
				//memoryStored = false;
				//textBox_memory.Text = "";
				break;
			case OP.CLRENTRY:
				//textBox_display.Text = currentValue.ToString();
				soonTxt = currentValue.ToString();
				displayText();
				endingOp = true;
				break;
			case OP.BACKSPACE:
				if(!hasNum)
				{
					//textBox_display.Text = currentValue.ToString();
					soonTxt = currentValue.ToString();
					displayText();
					endingOp = true;
				}
				else
					if(currentOperation != OP.NONE)
					{
						//textBox_display.Text = textBox_display.Text.Substring(0, textBox_display.Text.Length - 2);
						soonTxt = soonTxt.Substring(0, soonTxt.Length - 1);
						displayText();
						if(soonTxt.Length <= TO_NUM)
							hasNum = false;
					}
					else
						if(soonTxt != "0")
						{
							//textBox_display.Text = textBox_display.Text.Substring(0, textBox_display.Text.Length - 1);
							soonTxt = soonTxt.Substring(0, soonTxt.Length - 1);
							displayText();
							if(soonTxt.Length == 0)
							{
								hasNum = false;
								//textBox_display.Text = "0";
								soonTxt = "0";
								displayText();
							}
						}
				break;
			case OP.PER:
				double newDigits = getDigits() * 0.01 * currentValue;
				if(displayedOp)
					soonTxt = soonTxt.Substring(0, TO_NUM) + newDigits.ToString("g");
				else
					soonTxt = newDigits.ToString("g");
				displayText();
				break;
			case OP.MSTORE:
				if(currentValue != 0)
				{
					endingOp = true;
					//textBox_display.Text = currentValue.ToString();
					soonTxt = currentValue.ToString();
					displayText();
					hasNum = false;
					memoryValue = currentValue;
					memoryStored = true;
					textBox_memory.Text = "M";
				}
				else
					if(currentOperation == OP.NONE && soonTxt != "0")
					{
						endingOp = true;
						//textBox_display.Text = currentValue.ToString();
						soonTxt = currentValue.ToString();
						displayText();
						hasNum = false;
						memoryValue = Convert.ToDouble(soonTxt);
						memoryStored = true;
						textBox_memory.Text = "M";
					}
				break;
			case OP.MADD:
				if(memoryStored)
				{
					endingOp = true;
					//textBox_display.Text = currentValue.ToString();
					soonTxt = currentValue.ToString();
					displayText();
					hasNum = false;
					memoryValue += currentValue;
				}
				break;
			case OP.MCLEAR:
				textBox_memory.Text = "";
				memoryValue = 0;
				memoryStored = false;
				break;
			case OP.MRECALL:
				if(memoryStored && memoryValue != 0)
				{
					//different cases
					if(hasNum && currentOperation != OP.NONE) //typed an op and a num but not pressed equal.. hasNum && currentOperation != OP.NONE 
					{ //replace current typed num with the recalled num
						endingOp = true;

						//textBox_display.Text = textBox_display.Text.Substring(0, TO_NUM);
						//textBox_display.Text += memoryValue.ToString();
						soonTxt = soonTxt.Substring(0, TO_NUM);
						soonTxt += memoryValue.ToString();
						displayText();

						hasNum = true;
					}
					else
						if(hasNum && currentOperation == OP.NONE) //typed a num but no op.. hasNum && currentOperation == OP.NONE 
						{ //replace current typed num with the recalled num
							endingOp = true;
							//textBox_display.Text = memoryValue.ToString();
							soonTxt = memoryValue.ToString();
							displayText();
							hasNum = true;
						}
						else
							if(currentOperation == OP.NONE)//beginning with 0, !hasNum, currentOperation == OP.NONE
							{ //replace 0 with the recalled num
								endingOp = true;
								//textBox_display.Text = memoryValue.ToString();
								soonTxt = memoryValue.ToString();
								displayText();
								hasNum = true;
							}
							else //typed an op but not a num.. !hasNum, currentOperation != OP.NONE
							{ //enter the recalled number as the num
								endingOp = true;

								if(displayedOp)
								{
									//textBox_display.Text += memoryValue.ToString();
									soonTxt += memoryValue.ToString();
									displayText();
									hasNum = true;
								}
								else
								{
									//textBox_display.Text = memoryValue.ToString();
									soonTxt = memoryValue.ToString();
									displayText();
									currentValue = Convert.ToDouble(soonTxt);
									hasNum = false;
								}
							}
				}
				break;
			default:
				break;
			}
		}

		private void button_0_Click(object sender, EventArgs e)
		{
			appendToDisplay("0");
			button_equals.Focus();
		}

		private void button_1_Click(object sender, EventArgs e)
		{
			appendToDisplay("1");
			button_equals.Focus();
		}

		private void button_2_Click(object sender, EventArgs e)
		{
			appendToDisplay("2");
			button_equals.Focus();
		}

		private void button_3_Click(object sender, EventArgs e)
		{
			appendToDisplay("3");
			button_equals.Focus();
		}

		private void button_4_Click(object sender, EventArgs e)
		{
			appendToDisplay("4");
			button_equals.Focus();
		}

		private void button_5_Click(object sender, EventArgs e)
		{
			appendToDisplay("5");
			button_equals.Focus();
		}

		private void button_6_Click(object sender, EventArgs e)
		{
			appendToDisplay("6");
			button_equals.Focus();
		}

		private void button_7_Click(object sender, EventArgs e)
		{
			appendToDisplay("7");
			button_equals.Focus();
		}

		private void button_8_Click(object sender, EventArgs e)
		{
			appendToDisplay("8");
			button_equals.Focus();
		}

		private void button_9_Click(object sender, EventArgs e)
		{
			appendToDisplay("9");
			button_equals.Focus();
		}

		private void button_decimal_Click(object sender, EventArgs e)
		{
			//if no current decimal point
			if(!soonTxt.Contains("."))
				appendToDisplay(".");
		}

		private void button_add_Click(object sender, EventArgs e)
		{
			setOperation(OP.ADD);
			button_equals.Focus();
		}

		private void button_subtract_Click(object sender, EventArgs e)
		{
			setOperation(OP.SUB);
			button_equals.Focus();
		}

		private void button_divide_Click(object sender, EventArgs e)
		{
			setOperation(OP.DIV);
			button_equals.Focus();
		}

		private void button_multiply_Click(object sender, EventArgs e)
		{
			setOperation(OP.MUL);
			button_equals.Focus();
		}

		private void button_equals_Click(object sender, EventArgs e)
		{
			setOperation(OP.EQU);
			button_equals.Focus();
		}

		private void button_reciprocal_Click(object sender, EventArgs e)
		{
			setOperation(OP.RECIP);
			button_equals.Focus();
		}

		private void button_clearAll_Click(object sender, EventArgs e)
		{
			setOperation(OP.CLR);
			button_equals.Focus();
		}

		private void button_clearEntry_Click(object sender, EventArgs e)
		{
			setOperation(OP.CLRENTRY);
			button_equals.Focus();
		}

		private void button_sqrt_Click(object sender, EventArgs e)
		{
			setOperation(OP.SQRT);
			button_equals.Focus();
		}

		private void button_inverse_Click(object sender, EventArgs e)
		{
			setOperation(OP.INV);
			button_equals.Focus();
		}

		private void button_backspace_Click(object sender, EventArgs e)
		{
			setOperation(OP.BACKSPACE);
			button_equals.Focus();
		}

		private void button_memoryClear_Click(object sender, EventArgs e)
		{
			setOperation(OP.MCLEAR);
			button_equals.Focus();
		}

		private void button_memoryRecall_Click(object sender, EventArgs e)
		{
			setOperation(OP.MRECALL);
			button_equals.Focus();
		}

		private void button_memoryStore_Click(object sender, EventArgs e)
		{
			setOperation(OP.MSTORE);
			button_equals.Focus();
		}

		private void button_memoryAdd_Click(object sender, EventArgs e)
		{
			setOperation(OP.MADD);
			button_equals.Focus();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			double currentValue_bak = currentValue;
			equateDisplay();
			Clipboard.SetText(currentValue.ToString());
			currentValue = currentValue_bak;
			button_equals.Focus();
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			double result;

			if(Double.TryParse(Clipboard.GetText(), out result))
			{
				//different cases
				if(hasNum && currentOperation != OP.NONE) //typed an op and a num but not pressed equal.. hasNum && currentOperation != OP.NONE 
				{ //replace current typed num with the recalled num
					endingOp = true;
					//textBox_display.Text = textBox_display.Text.Substring(0, TO_NUM);
					//textBox_display.Text += result.ToString();
					soonTxt = soonTxt.Substring(0, TO_NUM);
					soonTxt += result.ToString();
					displayText();
					hasNum = true;
				}
				else
					if(hasNum && currentOperation == OP.NONE) //typed a num but no op.. hasNum && currentOperation == OP.NONE 
					{ //replace current typed num with the recalled num
						endingOp = true;
						//textBox_display.Text = result.ToString();
						soonTxt = result.ToString();
						displayText();
						hasNum = true;
					}
					else
						if(currentOperation == OP.NONE)//beginning with 0, !hasNum, currentOperation == OP.NONE
						{ //replace 0 with the recalled num
							endingOp = true;
							//textBox_display.Text = result.ToString();
							soonTxt = result.ToString();
							displayText();
							hasNum = true;
						}
						else //typed an op but not a num.. !hasNum, currentOperation != OP.NONE
						{ //enter the recalled number as the num
							endingOp = true;
							if(currentOperation == OP.ADD || currentOperation == OP.SUB || currentOperation == OP.MUL || currentOperation == OP.DIV)
							{
								//textBox_display.Text += result.ToString();
								soonTxt += result.ToString();
								displayText();
								hasNum = true;
							}
							else
							{
								//textBox_display.Text = result.ToString();
								soonTxt = result.ToString();
								displayText();
								currentValue = Convert.ToDouble(soonTxt);
								hasNum = false;
							}
						}
			}
			button_equals.Focus();
		}

		private void helpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var helpForm = new Help();
			helpForm.ShowDialog();
			helpForm.Dispose();
			helpForm = null;
			button_equals.Focus();
		}

		private void digitGroupingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(dgtGrouping.Checked)
			{
				dgtGrouping.Checked = false;
			}
			else
			{
				dgtGrouping.Checked = true;
			}

			displayText();
			button_equals.Focus();
		}

		private void button_equals_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.D0)
				appendToDisplay("0");

			else
			if (e.KeyChar == (char)Keys.D1)
				appendToDisplay("1");

			else
			if (e.KeyChar == (char)Keys.D2)
				appendToDisplay("2");

			else
			if (e.KeyChar == (char)Keys.D3)
				appendToDisplay("3");

			else
			if (e.KeyChar == (char)Keys.D4)
				appendToDisplay("4");

			else
			if (e.KeyChar == (char)Keys.D5)
				appendToDisplay("5");

			else
			if (e.KeyChar == (char)Keys.D6)
				appendToDisplay("6");

			else
			if (e.KeyChar == (char)Keys.D7)
				appendToDisplay("7");

			else
			if (e.KeyChar == (char)Keys.D8)
				appendToDisplay("8");

			else
			if (e.KeyChar == (char)Keys.D9)
				appendToDisplay("9");

			else
				if(e.KeyChar == (char)46 && !soonTxt.Contains("."))
					appendToDisplay(".");

			else
			if (e.KeyChar == (char)Keys.Enter)
				setOperation(OP.EQU);

			else
			if(e.KeyChar == (char)43)
				setOperation(OP.ADD);

			else
			if(e.KeyChar == (char)45)
				setOperation(OP.SUB);

			else
			if(e.KeyChar == (char)42)
				setOperation(OP.MUL);

			else
			if(e.KeyChar == (char)47)
				setOperation(OP.DIV);
		}

		private void textBox_memory_MouseDown(object sender, MouseEventArgs e)
		{
			button_equals.Focus();
		}

		private void button_percentage_Click(object sender, EventArgs e)
		{
			setOperation(OP.PER);
			button_equals.Focus();
		}
	}
}
