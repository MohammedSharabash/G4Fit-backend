<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Response.aspx.cs" Inherits="Response" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
     <link href="CSS/bootstrap.min.css" rel="stylesheet" />

</head>
<body>
    <form id="form1" runat="server">
   <div class="container wrapper">
           
            <div class="row cart-body">
             
                <div class="col-lg-6 ">
                    <!--SHIPPING METHOD-->
                    <div class="panel panel-info" >
                        <div class="panel-heading">Response details</div>
                        <div class="panel-body" runat="server" id="pnlpaymentsuccessinfo">
                            <div class="form-group">
                                <div class="col-md-12" runat="server" id="dvpayment">
                                                                     
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-12"><strong>PaymentId:</strong></div>
                                <div class="col-md-12">
                               <asp:TextBox ID="txtPaymentId" ReadOnly="true" runat="server"  class="form-control" > </asp:TextBox>
                                </div>

                                 <div class="col-md-12"><strong>TrackId:</strong></div>
                                <div class="col-md-12">
                               <asp:TextBox ID="txtTrackId" runat="server" ReadOnly="true"  class="form-control"    > </asp:TextBox>
                                </div>
                                  
                                 <div class="col-md-12"><strong>Result:</strong></div>
                                <div class="col-md-12">
                               <asp:TextBox ID="txtResult" runat="server"  ReadOnly="true" class="form-control"   > </asp:TextBox>
                                </div>
                                  
                                 <div class="col-md-12"><strong>amount:</strong></div>
                                <div class="col-md-12">
                               <asp:TextBox ID="txtamount" runat="server"  ReadOnly="true" class="form-control"    > </asp:TextBox>
                                </div>

                                <div class="form-group">
                                <div class="col-md-12">
                                    <h4>Thanks for using PG </h4>                                   
                                </div>
                            </div>
<%--                                <div class="col-md-12"><strong>ResponseHash:</strong></div>
                                <div class="col-md-12">
                               <asp:TextBox ID="txtResponseHash" runat="server"  class="form-control"    > </asp:TextBox>
                                </div>

                                 <div class="col-md-12"><strong>PGResponseHash:</strong></div>
                                <div class="col-md-12">
                               <asp:TextBox ID="txtPGResponseHash" runat="server"  class="form-control"    > </asp:TextBox>
                                </div>--%>
                            </div>
                           
                            </div>
                        <div class="form-group" runat="server" id="pnlpaymentunsuccessinfo">
                                 <div class="col-md-12"><strong style="padding:15px 0px; ">Payment unsuccessful</strong></div>
                        </div>
                                       <div class="clearfix"></div>                          
                    </div>
                    </div>
                    
         
                </div>
                   
            </div>
            <div class="row cart-footer">
        
            </div>
   
    </form>
</body>
</html>
