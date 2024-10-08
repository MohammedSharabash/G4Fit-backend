<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Payment2.aspx.cs" Inherits="Payment2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    	
    <title></title>


    <link href="CSS/bootstrap.min.css" rel="stylesheet" />

<%--<script src="//netdna.bootstrapcdn.com/bootstrap/3.2.0/js/bootstrap.min.js">--%>
<%--</script>--%>
<%--<script src="//code.jquery.com/jquery-1.11.1.min.js">--%>
<%--    </script>--%>

</head>
<body>
    <form id="form1" runat="server">
   <div class="container wrapper">
            <div class="row cart-head">
                <div class="container">
                <div class="row">
                    <p></p>
                </div>
            
                <div class="row">
                    <p></p>
                </div>
                </div>
            </div>    
            <div class="row cart-body">
               <%-- <form class="form-horizontal" method="post" action="../php_plugin/payment.php">--%>
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-12 col-md-push-6 col-sm-push-6">
                  <div class="panel panel-info">
                        <div class="panel-heading">Order</div>
                        <div class="panel-body">
                            <div class="form-group">
                                <div class="col-md-12">
                                    <h4>Order Details</h4>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-12"><strong>Order ID</strong></div>
                                <div class="col-md-12">
                                   <%-- <input type="text" class="form-control" name="trackid" value="<?php echo rand(); ?>" />--%>
                                     <asp:TextBox ID="txtTrackid" runat="server"   ReadOnly="true"  class="form-control" >   </asp:TextBox>
                                </div>
                            </div>		
                            <div class="form-group">
                                <div class="col-md-6 col-xs-12">
                                    <strong>Currency:</strong>
                                    <%--<input type="text" name="currency" class="form-control" value="SAR" />--%>
                                     <asp:TextBox ID="txtCurrency" runat="server"  class="form-control" value="SAR"  > </asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rqfvCurrency"  ForeColor="Red" runat="server" ControlToValidate="txtCurrency" CssClass="validator" ErrorMessage="Required!"></asp:RequiredFieldValidator>
                                </div>
                                <div class="span1"></div>
                                
                            </div>							
							  <div class="form-group">
                                <div class="col-md-6 col-xs-12">
                                    <strong>Amount:</strong>
                                   <%-- <input type="text" name="amount" class="form-control" value="1.00" />--%>
                                    <asp:TextBox ID="txtAmount" runat="server"  class="form-control" value="1.00"  > </asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rqfvAmount"  ForeColor="Red" runat="server" ControlToValidate="txtAmount" CssClass="validator" ErrorMessage="Required!"></asp:RequiredFieldValidator>
                                </div>
                                <div class="span1"></div>                                
                            </div>
                           
                            
                           
                            <div class="form-group">
                                <div class="col-md-6 col-sm-6 col-xs-12">
                                  <%--  <button type="submit" class="btn btn-primary btn-submit-fix">Place Order</button>--%>
                                    <asp:Button ID="btnsubmit" class="btn btn-primary btn-submit-fix"     runat="server" Text="Place Order" OnClick="btnsubmit_Click" />
                                </div>
                            </div>
                            
                          
                        </div>
                      
                    </div>
                     <%--<div class="form-group">
                     <asp:Label ID="Label3" runat="server" Text="Label"></asp:Label>
                     </div>--%>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-12 col-md-pull-6 col-sm-pull-6">
                    <!--SHIPPING METHOD-->
                    <div class="panel panel-info">
                        <div class="panel-heading">Address</div>
                        <div class="panel-body">
                            <div class="form-group">
                                <div class="col-md-12">
                                    <h4>Shipping Address</h4>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-12"><strong>Country:</strong></div>
                                <div class="col-md-12">
                               <asp:TextBox ID="txtCountry" runat="server"  class="form-control" value="SA"  > </asp:TextBox>
                                     <asp:RequiredFieldValidator ID="rqfCountry" runat="server" ForeColor="Red"  ControlToValidate="txtCountry" CssClass="validator" ErrorMessage="Required!"></asp:RequiredFieldValidator>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-6 col-xs-12">
                                    <strong>First Name:</strong>
                                 
                                  <asp:TextBox ID="txtfirst_name" runat="server"  class="form-control"> </asp:TextBox>
                                  <asp:RequiredFieldValidator ID="Rqfvfirst_name" ForeColor="Red" runat="server" ControlToValidate="txtfirst_name" CssClass="validator" ErrorMessage="Required!"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="Regfirst_name" ForeColor="Red" Display = "Dynamic" runat="server" ErrorMessage="only characters allowed" ControlToValidate="txtfirst_name" ValidationExpression="^[A-Za-z]*$" ></asp:RegularExpressionValidator>
                                </div>
                                <div class="span1"></div>
                                <div class="col-md-6 col-xs-12">
                                    <strong>Last Name:</strong>
                               
                                      <asp:TextBox ID="txtlast_name" runat="server"  class="form-control"> </asp:TextBox>
                                  <asp:RequiredFieldValidator ID="Rqfvlast_name"  ForeColor="Red" runat="server" ControlToValidate="txtlast_name" CssClass="validator" ErrorMessage="Required!"></asp:RequiredFieldValidator>
                                     <asp:RegularExpressionValidator ID="Reglast_name" ForeColor="Red" Display = "Dynamic" runat="server" ErrorMessage="only characters allowed" ControlToValidate="txtlast_name" ValidationExpression="^[A-Za-z]*$" ></asp:RegularExpressionValidator>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-12"><strong>Address:</strong></div>
                                <div class="col-md-12">
                                 
                                     <asp:TextBox ID="txtaddress" runat="server"  class="form-control"> </asp:TextBox>
                                    <asp:RequiredFieldValidator ID="Rqfvadddress" ForeColor="Red" runat="server" ControlToValidate="txtaddress" CssClass="validator" ErrorMessage="Required!"  ></asp:RequiredFieldValidator>
                                   <%--  <asp:RegularExpressionValidator ID="RegAddress" ForeColor="Red" Display = "Dynamic" runat="server" ErrorMessage="only characters allowed" ControlToValidate="txtaddress" ValidationExpression="^[A-Za-z]*$" ></asp:RegularExpressionValidator>--%>
                                </div>
                            </div>
							
							
							
                            <div class="form-group">
                                <div class="col-md-12"><strong>City:</strong></div>
                                <div class="col-md-12">
                                
                                     <asp:TextBox ID="txtcity" runat="server"  class="form-control"> </asp:TextBox>
                                    <asp:RequiredFieldValidator ID="Rqfvcity" ForeColor="Red" runat="server" ControlToValidate="txtcity" CssClass="validator" ErrorMessage="Required!"></asp:RequiredFieldValidator>
                                    <%--<asp:RegularExpressionValidator ID="Regcity" ForeColor="Red" Display = "Dynamic" runat="server" ErrorMessage="only characters allowed" ControlToValidate="txtcity" ValidationExpression="^[A-Za-z]*$" ></asp:RegularExpressionValidator>--%>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-12"><strong>State:</strong></div>
                                <div class="col-md-12">
                                 <asp:TextBox ID="txtstate" runat="server"  class="form-control"> </asp:TextBox>
                                    <asp:RequiredFieldValidator ID="Rqfvstate" ForeColor="Red" runat="server" ControlToValidate="txtstate" CssClass="validator" ErrorMessage="Required!" ></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="Regtxtstate" ForeColor="Red" Display = "Dynamic" runat="server" ErrorMessage="only characters allowed" ControlToValidate="txtstate" ValidationExpression="^[A-Za-z]*$" ></asp:RegularExpressionValidator>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-12"><strong>Zip / Postal Code:</strong></div>
                                <div class="col-md-12">
                                <asp:TextBox ID="txtzip" runat="server"   MaxLength="6" ControlToValidate="txtzip" class="form-control" ErrorMessage="Enter Valid zip" ValidationExpression="^[0-9]{10,12}$"   >  </asp:TextBox>
                                <%--<asp:RequiredFieldValidator ID="Rqfvzip" runat="server" ForeColor="Red"  MaxLength="6" ControlToValidate="txtzip" CssClass="validator" ErrorMessage="Required!"></asp:RequiredFieldValidator>--%>
                                <asp:RegularExpressionValidator    ID="RequiredFieldValidatorZip"   ForeColor="Red" ControlToValidate="txtZip" ValidationExpression="^(\d{6}|\d{6}\-\d{5})$"  ErrorMessage="Zip code must be numeric." Display="dynamic" runat="server"></asp:RegularExpressionValidator>
                               <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ForeColor="Red"  ErrorMessage="Required!" ControlToValidate="txtZip"></asp:RequiredFieldValidator>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-12"><strong>Phone Number:</strong></div>
                                <div class="col-md-12"><asp:TextBox ID="txtPhoneno" runat="server"  MaxLength="12" class="form-control"></asp:TextBox>
                                   <asp:RequiredFieldValidator ID="RfaPhoneno" ForeColor="Red" runat="server" ControlToValidate="txtPhoneno" CssClass="validator" ErrorMessage="Required!" ></asp:RequiredFieldValidator>
                                 <asp:RegularExpressionValidator ID="Revphonenumber" ForeColor="Red" runat="server" ControlToValidate="txtPhoneno" CssClass="validator" ErrorMessage="Enter Valid Cell Number" ValidationExpression="^[0-9]{10,12}$"></asp:RegularExpressionValidator>
                                </div>

                            </div>
                            <div class="form-group"> 
                                <div class="col-md-12"><strong>Email Address:</strong></div>
                                <div class="col-md-12"><asp:TextBox ID="txtcustomerEmail" runat="server"  class="form-control"> </asp:TextBox>
                                <%--<asp:RequiredFieldValidator ID="RefcustomerEmail" runat="server" ControlToValidate="txtcustomerEmail" CssClass="validator" ErrorMessage="Required!" ></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="Rfvemail" runat="server" ControlToValidate="txtcustomerEmail" CssClass="validator" ErrorMessage="Invalid EmailID" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>--%>
                                <asp:RegularExpressionValidator ID="RefcustomerEmail" runat="server" ControlToValidate="txtcustomerEmail"
                                    ForeColor="Red" ValidationExpression="^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"
                                    Display = "Dynamic" ErrorMessage = "Invalid email address"/>
                                <asp:RequiredFieldValidator ID="Rfvemail" runat="server" ControlToValidate="txtcustomerEmail"
                                    ForeColor="Red" Display = "Dynamic" ErrorMessage = "Required" />
                                    </div>
                            </div>
                        </div>
                    </div>
                    <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
                </div>
          </div>
            <div class="row cart-footer">
        
            </div>
    </div>
    </form>
</body>
</html>


 