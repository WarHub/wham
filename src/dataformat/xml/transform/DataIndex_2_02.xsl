<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:bsi="http://www.battlescribe.net/schema/dataIndexSchema"
                xmlns="http://www.battlescribe.net/schema/dataIndexSchema"
                exclude-result-prefixes="bsi">

    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="/bsi:dataIndex/@battleScribeVersion">
        <xsl:attribute name="battleScribeVersion">2.02</xsl:attribute>
    </xsl:template>
    
</xsl:stylesheet>
