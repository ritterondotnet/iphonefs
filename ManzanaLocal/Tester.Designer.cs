/*--------------------------------------------------------------------*\
 * This source file is subject to the GPLv3 license that is bundled   *
 * with this package in the file COPYING.                             *
 * It is also available through the world-wide-web at this URL:       *
 * http://www.gnu.org/licenses/gpl-3.0.txt                            *
 * If you did not receive a copy of the license and are unable to     *
 * obtain it through the world-wide-web, please send an email         *
 * to bsd-license@lokkju.com so we can send you a copy immediately.   *
 *                                                                    *
 * @category   iPhone                                                 *
 * @package    iPhone File System for Windows                         *
 * @copyright  Copyright (c) 2010 Lokkju Inc. (http://www.lokkju.com) *
 * @license    http://www.gnu.org/licenses/gpl-3.0.txt GNU v3 Licence *
 *                                                                    *
 * $Revision::                            $:  Revision of last commit *
 * $Author::                              $:  Author of last commit   *
 * $Date::                                $:  Date of last commit     *
 * $Id::                                                            $ *
\*--------------------------------------------------------------------*/
namespace ManzanaLocal
{
    partial class Tester
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Tester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Name = "Tester";
            this.Text = "Tester";
            this.Load += new System.EventHandler(this.Tester_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}