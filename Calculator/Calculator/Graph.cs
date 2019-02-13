using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Calculator
{
    public class Graph
    {
        private void showGraph()
        {
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = @"
<html>
<body>
<script src = ""https://www.desmos.com/api/v1.0/calculator.js?apiKey=dcb31709b452b1cf9dc26972add0fda6""></script>
 <div id = ""calculator"" style = ""width: 600px; height: 400px;""></div>
    <script>
      var elt = document.getElementById('calculator');
            var calculator = Desmos.GraphingCalculator(elt, { expressions: false, settingsMenu: false, zoomButtons: false});
            calculator.setExpression({ id: 'graph1', latex: 'y=x^2'});
</script>
</body>
</html> ";
            //graph.Source = htmlSource;
        }
    }
}
